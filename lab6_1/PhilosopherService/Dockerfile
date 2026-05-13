FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PhilosopherService.csproj", "./"]
RUN dotnet restore "PhilosopherService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PhilosopherService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhilosopherService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhilosopherService.dll"]


