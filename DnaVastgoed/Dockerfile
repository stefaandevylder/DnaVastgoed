# SETUP - Base
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# BUILD - Copy the files
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
RUN dotnet build "DnaVastgoed.csproj" -c Release -o /app/build

RUN dotnet dev-certs https

# PUBLISH - The project
FROM build AS publish
RUN dotnet publish "DnaVastgoed.csproj" -c Release -o /app/publish

# RUN - The project
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DnaVastgoed.dll"]
