# Stage 1: Build (Builder)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy the repository root so the project references can resolve correctly.
COPY . .

# Restore the API project and its referenced projects.
RUN dotnet restore src/AgentFlow.API/AgentFlow.API.csproj

# Publish the API project.
RUN dotnet publish src/AgentFlow.API/AgentFlow.API.csproj -c Release -o /app/publish

# Stage 2: Runtime (Final Image)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

# Assembly name matches the project file: AgentFlow.API.dll
ENTRYPOINT ["dotnet", "AgentFlow.API.dll"]