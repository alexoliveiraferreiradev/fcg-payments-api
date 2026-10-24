FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


RUN dotnet tool install --global dotnet-ef --version 9.0.15
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["src/Fcg.Payments.API/Fcg.Payments.API.csproj", "Fcg.Payments.API/"]
COPY ["src/Fcg.Payments.Application/Fcg.Payments.Application.csproj", "Fcg.Payments.Application/"]
COPY ["src/Fcg.Payments.Domain/Fcg.Payments.Domain.csproj", "Fcg.Payments.Domain/"]
COPY ["src/Fcg.Payments.Infrastructure/Fcg.Payments.Infrastructure.csproj", "Fcg.Payments.Infrastructure/"]

RUN dotnet restore "./Fcg.Payments.API/Fcg.Payments.API.csproj"

COPY src/ .
WORKDIR "/src/Fcg.Payments.API"

# Sem --no-restore: o publish refaz o restore no Linux e não depende de nenhum obj/ que venha no contexto.
RUN dotnet publish "./Fcg.Payments.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet ef migrations bundle --no-build --configuration $BUILD_CONFIGURATION --self-contained -r linux-x64 -o /app/efbundle

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /app/efbundle ./efbundle

ENTRYPOINT []
CMD ["dotnet", "Fcg.Payments.API.dll"]
