# =========================================================================
# Stage 1: Build React Dashboard (Node.js 20)
# =========================================================================
FROM node:20-alpine AS frontend-builder
WORKDIR /app/frontend

COPY vitaltemp-dashboard/package*.json ./
RUN npm install

COPY vitaltemp-dashboard/ ./
RUN npm run build

# =========================================================================
# Stage 2: Build & Publish .NET 8 Web API
# =========================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-builder
WORKDIR /app

# Copy csproj files for optimal layer caching
COPY VitalTemp.sln* ./
COPY VitalTemp.slnx* ./
COPY src/VitalTemp.Domain/VitalTemp.Domain.csproj src/VitalTemp.Domain/
COPY src/VitalTemp.Application/VitalTemp.Application.csproj src/VitalTemp.Application/
COPY src/VitalTemp.Infrastructure/VitalTemp.Infrastructure.csproj src/VitalTemp.Infrastructure/
COPY src/VitalTemp.API/VitalTemp.API.csproj src/VitalTemp.API/

RUN dotnet restore src/VitalTemp.API/VitalTemp.API.csproj

# Copy the entire source code
COPY src/ src/
COPY phoenix_heat_health_risk.csv ./

# Copy built frontend assets into API wwwroot
COPY --from=frontend-builder /app/src/VitalTemp.API/wwwroot/ src/VitalTemp.API/wwwroot/

RUN dotnet publish src/VitalTemp.API/VitalTemp.API.csproj -c Release -o /app/publish

# =========================================================================
# Stage 3: Production Runtime (.NET 8 ASP.NET Core)
# =========================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Copy published application and dataset
COPY --from=backend-builder /app/publish .
COPY --from=backend-builder /app/phoenix_heat_health_risk.csv .

EXPOSE 5162
EXPOSE 80
EXPOSE 8080

ENTRYPOINT ["dotnet", "VitalTemp.API.dll"]
