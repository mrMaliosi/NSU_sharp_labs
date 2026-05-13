FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TableService.csproj", "./"]
RUN dotnet restore "TableService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "TableService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TableService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TableService.dll"]

