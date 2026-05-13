# Используем образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проекта Core (зависимость)
COPY DiningPhilosophers/src/DiningPhilosophers.Core/*.csproj ./DiningPhilosophers/src/DiningPhilosophers.Core/
COPY DiningPhilosophers/src/DiningPhilosophers.Core/ ./DiningPhilosophers/src/DiningPhilosophers.Core/

# Копируем файлы TableService
COPY TableService/TableService.csproj ./TableService/
COPY TableService/ ./TableService/

# Восстанавливаем зависимости
WORKDIR /src/TableService
RUN dotnet restore

# Собираем и публикуем приложение
RUN dotnet publish TableService.csproj -c Release -o /app/publish --no-restore

# Используем образ runtime для запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Устанавливаем curl для healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Открываем порт
EXPOSE 8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "TableService.dll"]

