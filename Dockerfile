FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["src/Fcg.Payments.API/Fcg.Payments.API.csproj", "Fcg.Payments.API/"]
COPY ["src/Fcg.Payments.Application/Fcg.Payments.Application.csproj", "Fcg.Payments.Application/"]
COPY ["src/Fcg.Payments.Domain/Fcg.Payments.Domain.csproj", "Fcg.Payments.Domain/"]
COPY ["src/Fcg.Payments.Infrastructure/Fcg.Payments.Infrastructure.csproj", "Fcg.Payments.Infrastructure/"]
RUN dotnet restore "./Fcg.Payments.API/Fcg.Payments.API.csproj"

COPY src/ .
WORKDIR "/src/Fcg.Payments.API"
RUN dotnet build "./Fcg.Payments.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet ef migrations bundle --self-contained -r linux-x64 -o /app/build/efbundle

FROM build AS publish
RUN dotnet publish "./Fcg.Payments.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root

COPY --from=build /app/build/efbundle ./efbundle
RUN chmod +x ./efbundle
USER app

ENTRYPOINT []
CMD ["dotnet", "Fcg.Payments.API.dll"]