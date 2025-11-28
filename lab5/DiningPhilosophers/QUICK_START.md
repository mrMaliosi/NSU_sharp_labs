# Быстрый старт - Полная инструкция

## Шаг 1: Установка PostgreSQL

### Windows

1. Скачайте PostgreSQL с официального сайта: https://www.postgresql.org/download/windows/
2. Установите PostgreSQL (запомните пароль для пользователя `postgres`)
3. Убедитесь, что служба PostgreSQL запущена (обычно запускается автоматически)

### Проверка установки

Откройте командную строку и выполните:
```bash
psql --version
```

Или через pgAdmin (графический интерфейс, устанавливается вместе с PostgreSQL).

## Шаг 2: Создание базы данных

### Вариант 1: Через командную строку (psql)

1. Откройте командную строку
2. Выполните:
```bash
psql -U postgres
```
3. Введите пароль пользователя postgres
4. Выполните SQL команду:
```sql
CREATE DATABASE "DiningPhilosophers";
\q
```

### Вариант 2: Через pgAdmin

1. Откройте pgAdmin
2. Подключитесь к серверу PostgreSQL
3. Правой кнопкой на "Databases" → "Create" → "Database"
4. Имя базы: `DiningPhilosophers`
5. Нажмите "Save"

## Шаг 3: Настройка строки подключения

Откройте файл `src/DiningPhilosophers.App/appsettings.json` и убедитесь, что строка подключения правильная:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DiningPhilosophers;Username=postgres;Password=ВАШ_ПАРОЛЬ"
  },
  ...
}
```

**Важно:** Замените `ВАШ_ПАРОЛЬ` на реальный пароль пользователя postgres.

Также обновите `src/DiningPhilosophers.View/appsettings.json` с той же строкой подключения.

## Шаг 4: Установка инструмента dotnet-ef

Выполните в командной строке:
```bash
dotnet tool install --global dotnet-ef
```

Проверьте установку:
```bash
dotnet ef --version
```

## Шаг 5: Создание и применение миграций

1. Перейдите в директорию проекта:
```bash
cd DiningPhilosophers
```

2. Создайте миграцию:
```bash
dotnet ef migrations add InitialCreate --project src/DiningPhilosophers.Core --startup-project src/DiningPhilosophers.App
```

3. Примените миграцию к базе данных:
```bash
dotnet ef database update --project src/DiningPhilosophers.Core --startup-project src/DiningPhilosophers.App
```

После этого в базе данных будут созданы таблицы:
- `SimulationRuns`
- `PhilosopherSnapshots`
- `ForkSnapshots`

## Шаг 6: Запуск симуляции

```bash
dotnet run --project src/DiningPhilosophers.App
```

**Важно:** При запуске в консоль будет выведен `RunId` (GUID), например:
```
RunId: 550e8400-e29b-41d4-a716-446655440000
```

**Скопируйте этот RunId** - он понадобится для просмотра состояния!

Симуляция будет работать в течение времени, указанного в `appsettings.json` (по умолчанию 10 секунд).

## Шаг 7: Просмотр состояния симуляции

После завершения симуляции (или во время работы) вы можете посмотреть состояние на любой момент времени:

```bash
dotnet run --project src/DiningPhilosophers.View -- --runId <ВАШ_RUNID> --delay <СЕКУНДЫ>
```

### Примеры:

1. Посмотреть состояние через 5 секунд после начала:
```bash
dotnet run --project src/DiningPhilosophers.View -- --runId 550e8400-e29b-41d4-a716-446655440000 --delay 5.0
```

2. Посмотреть состояние через 44.12 секунды:
```bash
dotnet run --project src/DiningPhilosophers.View -- --runId 550e8400-e29b-41d4-a716-446655440000 --delay 44.12
```

3. Также можно использовать числовой ID (если знаете):
```bash
dotnet run --project src/DiningPhilosophers.View -- --runId 1 --delay 5.0
```

## Шаг 8: Запуск тестов

Для запуска тестов с использованием InMemory базы данных:

```bash
dotnet test
```

## Проверка данных в базе

### Через psql:

```bash
psql -U postgres -d DiningPhilosophers
```

Затем выполните SQL запросы:
```sql
-- Посмотреть все симуляции
SELECT "Id", "RunId", "StartTime", "TotalPhilosophers", "Strategy" FROM "SimulationRuns";

-- Посмотреть снимки состояния философов для конкретной симуляции
SELECT * FROM "PhilosopherSnapshots" WHERE "SimulationRunId" = 1 ORDER BY "ElapsedSeconds";

-- Посмотреть снимки состояния вилок
SELECT * FROM "ForkSnapshots" WHERE "SimulationRunId" = 1 ORDER BY "ElapsedSeconds";
```

### Через pgAdmin:

1. Откройте pgAdmin
2. Подключитесь к базе `DiningPhilosophers`
3. Используйте Query Tool для выполнения SQL запросов

## Устранение проблем

### Ошибка подключения к базе данных

- Убедитесь, что PostgreSQL запущен
- Проверьте правильность пароля в строке подключения
- Убедитесь, что база данных `DiningPhilosophers` создана

### Ошибка "dotnet ef не найден"

- Установите инструмент: `dotnet tool install --global dotnet-ef`
- Перезапустите командную строку

### Ошибка при применении миграций

- Убедитесь, что база данных создана
- Проверьте права доступа пользователя postgres
- Убедитесь, что строка подключения правильная

## Полный пример работы

1. **Запустите симуляцию:**
   ```bash
   dotnet run --project src/DiningPhilosophers.App
   ```
   Скопируйте RunId из вывода.

2. **Подождите завершения симуляции** (или прервите через Ctrl+C)

3. **Просмотрите состояние на момент 5 секунд:**
   ```bash
   dotnet run --project src/DiningPhilosophers.View -- --runId <ВАШ_RUNID> --delay 5.0
   ```

4. **Просмотрите состояние на момент 10 секунд:**
   ```bash
   dotnet run --project src/DiningPhilosophers.View -- --runId <ВАШ_RUNID> --delay 10.0
   ```

Готово! Теперь вы можете просматривать состояние симуляции на любой момент времени.

