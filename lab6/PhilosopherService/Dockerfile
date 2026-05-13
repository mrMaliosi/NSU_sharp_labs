# Используем образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проекта Core (зависимость)
COPY DiningPhilosophers/src/DiningPhilosophers.Core/*.csproj ./DiningPhilosophers/src/DiningPhilosophers.Core/
COPY DiningPhilosophers/src/DiningPhilosophers.Core/ ./DiningPhilosophers/src/DiningPhilosophers.Core/

# Копируем файлы PhilosopherService
COPY PhilosopherService/PhilosopherService.csproj ./PhilosopherService/
COPY PhilosopherService/ ./PhilosopherService/

# Восстанавливаем зависимости
WORKDIR /src/PhilosopherService
RUN dotnet restore

# Собираем и публикуем приложение
RUN dotnet publish PhilosopherService.csproj -c Release -o /app/publish --no-restore

# Используем образ runtime для запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# Открываем порт (будет переопределен через переменные окружения)
EXPOSE 8081

# Запускаем приложение
ENTRYPOINT ["dotnet", "PhilosopherService.dll"]

