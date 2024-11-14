# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SmsGate.Balance.Hosting/SmsGate.Balance.Hosting.csproj", "SmsGate.Balance.Hosting/"]
COPY ["SmsGate.Balance.Components/SmsGate.Balance.Components.csproj", "SmsGate.Balance.Components/"]
COPY ["SmsGate.Balance.Domain/SmsGate.Balance.Domain.csproj", "SmsGate.Balance.Domain/"]
COPY ["SmsGate.Balance.Models/SmsGate.Balance.Models.csproj", "SmsGate.Balance.Models/"]
COPY ["Orleans.Sagas/Orleans.Sagas.csproj", "Orleans.Sagas/"]
COPY ["SmsGate.Shared/SmsGate.Shared.csproj", "SmsGate.Shared/"]
RUN dotnet restore "./SmsGate.Balance.Hosting/SmsGate.Balance.Hosting.csproj"
COPY . .
WORKDIR "/src/SmsGate.Balance.Hosting"
RUN dotnet build "./SmsGate.Balance.Hosting.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SmsGate.Balance.Hosting.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmsGate.Balance.Hosting.dll"]