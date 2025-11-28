# Инструкция по миграциям базы данных

## Создание миграций

Для создания миграций Entity Framework Core выполните следующие команды:

1. Убедитесь, что у вас установлен .NET SDK и PostgreSQL

2. Установите инструмент dotnet-ef (если еще не установлен):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
   
   Или восстановите локальные инструменты проекта:
   ```bash
   dotnet tool restore
   ```

3. Перейдите в директорию проекта:
   ```bash
   cd DiningPhilosophers
   ```

4. Создайте миграцию:
   ```bash
   dotnet ef migrations add InitialCreate --project src/DiningPhilosophers.Core --startup-project src/DiningPhilosophers.App
   ```

5. Примените миграцию к базе данных:
   ```bash
   dotnet ef database update --project src/DiningPhilosophers.Core --startup-project src/DiningPhilosophers.App
   ```

## Настройка базы данных

Перед запуском приложения убедитесь, что:

1. PostgreSQL установлен и запущен
2. Создана база данных `DiningPhilosophers`:
   ```sql
   CREATE DATABASE "DiningPhilosophers";
   ```

3. В файле `appsettings.json` указаны правильные параметры подключения:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=DiningPhilosophers;Username=postgres;Password=postgres"
     }
   }
   ```

## Использование

### Запуск симуляции

```bash
dotnet run --project src/DiningPhilosophers.App
```

При запуске симуляции в консоль будет выведен `RunId` (GUID), который можно использовать для просмотра состояния.

### Просмотр состояния симуляции

```bash
dotnet run --project src/DiningPhilosophers.View -- --runId <GUID> --delay <секунды>
```

Пример:
```bash
dotnet run --project src/DiningPhilosophers.View -- --runId 550e8400-e29b-41d4-a716-446655440000 --delay 44.12
```

Также поддерживается поиск по числовому ID:
```bash
dotnet run --project src/DiningPhilosophers.View -- --runId 1 --delay 44.12
```

