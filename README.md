# Fiap Cloud Games (FCG) - Payments API

Este microsserviço é responsável pela **simulação e processamento de pagamentos** de pedidos de jogos na plataforma **Fiap Cloud Games (FCG)**. Desenvolvido em **.NET 9**, o projeto adota os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)** para garantir alta performance, separação de responsabilidades e escalabilidade.

O serviço atua de forma **puramente reativa** dentro do fluxo de compra: ele não conhece pedidos nem bibliotecas, apenas recebe uma solicitação de cobrança, decide o resultado da transação e devolve essa resposta ao ecossistema. Quem orquestra o fluxo — criando o pedido, aguardando o pagamento e liberando os jogos — é a **Order API**.

---

## 🔄 Fluxo de Eventos e Responsabilidades

A **Payments API** não expõe Controllers ou Swagger; toda a sua interação ocorre de forma reativa, utilizando o **RabbitMQ** para o consumo e publicação de eventos de integração.

```mermaid
sequenceDiagram
    autonumber
    participant O as Order API
    participant P as Payments API

    O-->>P: OrderPlacedEvent
    P->>P: Simula a transação (90% aprovação)

    alt Aprovado
        P-->>O: PaymentProcessedEvent
        Note right of O: Finaliza o pedido e libera os jogos
    else Recusado
        P-->>O: PaymentFailedEvent
        Note right of O: Cancela o pedido
    end

    opt Falha na entrega após aprovação
        O-->>P: DeliveryFailedEvent
        P->>P: Registra o reembolso
    end
```

### 1. Processamento de Pagamentos (Simulação)
*   **Consome:** `IOrderPlacedIntegrationEvent`, publicado pela **Order API** no momento em que o pedido é criado.
*   **O que faz:** Ao receber este evento, a aplicação cria um registro de pagamento e executa uma **simulação** (com 90% de chance de aprovação; pedidos de valor zero são sempre aprovados). O meio de pagamento também é sorteado.
*   **Produz (Publica):**
    *   `IPaymentProcessedIntegrationEvent`: Disparado caso a simulação seja **aprovada**. A *Order API* o consome para finalizar o pedido e, em seguida, liberar os jogos na biblioteca do usuário.
    *   `IPaymentFailedIntegrationEvent`: Disparado caso a simulação seja **rejeitada**. A *Order API* o consome para cancelar o pedido, encerrando o fluxo de compra.

### 2. Reembolso (Estorno de Pagamentos)
*   **Consome:** `IDeliveryFailedIntegrationEvent`, publicado pela **Order API** quando a finalização do pedido falha *depois* de o pagamento já ter sido aprovado.
*   **O que faz:** Ao consumir este evento de falha, a API de Pagamentos encarrega-se de realizar o **reembolso** ao usuário. Ela localiza a transação aprovada correspondente ao pedido no banco de dados e altera seu estado interno, registrando a devolução do valor.

> [!IMPORTANT]
> Este consumer é a ponta compensatória de um **padrão Saga**. Como o pagamento é confirmado antes da entrega dos jogos, existe uma janela em que o cliente foi cobrado mas não recebeu o produto. A *Order API* detecta essa falha e publica o `IDeliveryFailedIntegrationEvent`; a *Payments API* responde estornando a cobrança, devolvendo o ecossistema a um estado consistente.

---

## 🛠️ Tecnologias e Bibliotecas

A API utiliza as seguintes tecnologias em sua pilha de desenvolvimento:

- **.NET 9.0**: Framework principal para o desenvolvimento da API.
- **Entity Framework Core 9.0**: ORM utilizado para escrita de dados e mapeamento das entidades no **SQL Server**.
- **MediatR (v14)**: Implementação do padrão Mediator para desacoplamento e gerenciamento de comandos internos da aplicação.
- **MassTransit**: Framework avançado de mensageria utilizado para abstrair e facilitar a comunicação com o **RabbitMQ**.
- **xUnit & Moq**: Frameworks para construção e execução de testes unitários e de integração.

### 📦 Shared Kernels (Pacotes Compartilhados da FCG)
Projetos criados internamente e utilizados para padronizar e compartilhar recursos base:
- **Fcg.Core.Abstractions**: Biblioteca de blocos de construção para Domain-Driven Design (DDD) em projetos .NET.
- **Fcg.Core.SharedContracts**: Contratos de eventos compartilhados para comunicação assíncrona entre os microsserviços da plataforma FCG (ex: RabbitMQ / MassTransit).

---

## 🏗️ Arquitetura da Solução

A API de Payments segue a estrutura de **Clean Architecture** para proteger o domínio e separar as responsabilidades do sistema:

```
src/
├── Fcg.Payments.Domain         # Entidades de Domínio, ValueObjects e Interfaces
├── Fcg.Payments.Application    # Casos de Uso (Commands), DTOs e Handlers do MediatR
├── Fcg.Payments.Infrastructure # Acesso a Dados (EF Core), Mapeamentos e Implementação de Repositórios
└── Fcg.Payments.API            # Host de Mensageria, Consumers do MassTransit e Inicialização (Program.cs)
```

### Detalhes das Camadas

#### 1. `Fcg.Payments.Domain` (Domínio)
Contém as regras de validação, entidades e enums de status da transação. Define a entidade agregada `Payment` (com comportamentos ricos como Aprovar, Rejeitar e Reembolsar) e o objeto de valor `Price`.

#### 2. `Fcg.Payments.Application` (Aplicação)
Contém os casos de uso representados por comandos (`ProcessPaymentCommand`, `RefundPaymentCommand`) e os respectivos Handlers (processadores do MediatR) encarregados de orquestrar a execução, gravar nos repositórios e disparar os eventos finais.

#### 3. `Fcg.Payments.Infrastructure` (Infraestrutura)
Responsável pela persistência e pelo registro do histórico financeiro no **SQL Server** via `DbContext`. Possui mapeamentos explícitos de tabelas e a implementação do padrão Repository com Unit of Work.

#### 4. `Fcg.Payments.API` (Apresentação / Worker)
O ponto de entrada e "worker" da aplicação. Ele inicializa os **Consumers do MassTransit** (`OrderPlacedEventConsumer` e `DeliveryFailedEventConsumer`), ouve a fila ininterruptamente e traduz os eventos recebidos em chamadas (via MediatR) para a camada de aplicação.

---

## ⚙️ Configuração e Variáveis de Ambiente

Para o funcionamento correto da API de Pagamentos, as credenciais de banco e do mensageiro devem estar configuradas no `appsettings.json` (quando rodando localmente):

```json
{
  "ConnectionStrings": {
    "PaymentConnection": "Server=localhost;Database=Fcg_Payments;User Id=sa;Password=SuaSenhaSegura;TrustServerCertificate=True;MultipleActiveResultSets=true;",
    "RabbitMq": "amqp://guest:guest@localhost:5672"
  }
}
```

No **Kubernetes**, essas variáveis são injetadas de forma dinâmica e segura por meio de `ConfigMaps` e `Secrets`.

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- SDK do [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) instalado.
- Servidor SQL Server em execução e RabbitMQ disponível.

### Passo a Passo (Terminal)

1. **Restaurar Dependências e Compilar:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Aplicar Migrações do Banco de Dados (EF Core):**
   ```bash
   dotnet ef database update --project src/Fcg.Payments.Infrastructure/ --startup-project src/Fcg.Payments.API/
   ```

3. **Executar a API (Worker):**
   ```bash
   dotnet run --project src/Fcg.Payments.API/
   ```

> [!NOTE]
> Como o projeto é exclusivamente reativo, ao iniciar ele se conectará ao RabbitMQ e ficará rodando como um serviço de fundo no terminal aguardando novos eventos na fila. Ele **não abrirá o navegador nem exporá portas web locais** com endpoints Swagger.

---

## ☸️ Implantação no Kubernetes (k8s)

Os manifestos locais de infraestrutura para este serviço encontram-se dentro da pasta `/k8s`. Apenas para fins de validação local no cluster:

```bash
kubectl apply -f k8s/
```

> [!TIP]
> **Migrações Automáticas:** O manifesto `deployment.yaml` contém um *Init Container* configurado com o pacote `efbundle`. Ao aplicar a API no Kubernetes, as migrações serão executadas e concluídas automaticamente antes que a API inicie e comece a processar pagamentos na fila.

---

## 🧪 Testes Automatizados

O repositório possui uma base sólida de testes automatizados organizados na pasta `/tests`:
- **Fcg.Payments.Domain.Tests:** Testes unitários para validar a proteção das regras de domínio e criação do agregado `Payment`.
- **Fcg.Payments.Application.Tests:** Testes focados na lógica orquestrada pelos comandos e na garantia da correta produção de novos eventos.
- **Fcg.Payments.Infrastructure.Integration:** Testes em banco de dados in-memory para validação dos Repositórios e Mapeamentos do Entity Framework.

Para executar todos eles de uma só vez, utilize:
```bash
dotnet test
```
