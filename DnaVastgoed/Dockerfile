# SETUP - Base
FROM mcr.microsoft.com/dotnet/aspnet:6.0.14-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# BUILD - Copy the files
FROM mcr.microsoft.com/dotnet/sdk:6.0.406-bullseye-slim-amd64 AS build
WORKDIR /src
COPY . .
RUN dotnet build "DnaVastgoed.csproj" -c Release -o /app/build

# PUBLISH - The project
FROM build AS publish
RUN dotnet publish "DnaVastgoed.csproj" -c Release -o /app/publish

# RUN - The project
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DnaVastgoed.dll"]
