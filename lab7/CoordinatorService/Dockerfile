FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoordinatorService.csproj", "./"]
RUN dotnet restore "CoordinatorService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "CoordinatorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoordinatorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoordinatorService.dll"]

