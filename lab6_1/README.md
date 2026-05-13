# Система обедающих философов - Микросервисная архитектура

Реализация классической задачи "Обедающие философы" в виде микросервисной архитектуры на ASP.NET Core.

## Архитектура

Система состоит из:
- **Table Service** - центральный сервис, управляющий состоянием вилок и сбором метрик
- **Philosopher Service** - отдельный экземпляр для каждого философа (5 сервисов)

## Технологии

- ASP.NET Core 8.0 Web API
- Docker & Docker Compose
- REST API для межсервисного взаимодействия

## Структура проекта

```
lab6_1/
├── TableService/          # Сервис стола
│   ├── Controllers/        # API контроллеры
│   ├── Models/            # Модели данных
│   ├── Services/          # Бизнес-логика
│   └── Dockerfile
├── PhilosopherService/     # Сервис философа
│   ├── Controllers/        # API контроллеры
│   ├── Services/          # Логика философа
│   └── Dockerfile
└── docker-compose.yml      # Конфигурация Docker Compose
```

## API Контракты

### Table Service

#### POST /api/table/register
Регистрация философа
```json
{
  "philosopherId": "philosopher-1",
  "philosopherName": "Платон"
}
```

#### POST /api/table/take-fork
Взять вилку
```json
{
  "philosopherId": "philosopher-1",
  "forkId": 1
}
```

#### POST /api/table/release-fork
Освободить вилку
```json
{
  "philosopherId": "philosopher-1",
  "forkId": 1
}
```

#### POST /api/table/update-stats
Обновить статистику
```json
{
  "philosopherId": "philosopher-1",
  "mealsEaten": 10,
  "totalThinkingTime": 5000,
  "totalEatingTime": 3000
}
```

#### POST /api/table/exit
Уведомить о выходе философа
```json
{
  "philosopherId": "philosopher-1"
}
```

#### GET /health
Health check endpoint

## Запуск

### Требования
- Docker
- Docker Compose

### Команды

1. Собрать и запустить все сервисы:
```bash
docker-compose up --build
```

2. Запустить в фоновом режиме:
```bash
docker-compose up -d --build
```

3. Просмотр логов:
```bash
docker-compose logs -f
```

4. Остановка:
```bash
docker-compose down
```

## Конфигурация

### Table Service
- `PHILOSOPHERS_COUNT` - количество философов (по умолчанию: 5)
- `ASPNETCORE_URLS` - URL для прослушивания (по умолчанию: http://+:8080)

### Philosopher Service
- `PHILOSOPHER_NAME` - имя философа
- `PHILOSOPHER_ID` - уникальный идентификатор
- `LEFT_FORK_ID` - ID левой вилки
- `RIGHT_FORK_ID` - ID правой вилки
- `TABLE_SERVICE_URL` - URL сервиса стола
- `SIMULATION_DURATION_MINUTES` - длительность симуляции в минутах

## Поведение системы

1. Каждый философ регистрируется в Table Service при старте
2. Философы циклически выполняют действия:
   - Думают (1-3 секунды)
   - Пытаются взять левую вилку
   - Пытаются взять правую вилку
   - Если обе вилки получены - едят (0.5-2 секунды)
   - Освобождают вилки
3. Статистика обновляется периодически
4. После истечения времени симуляции философ отправляет финальную статистику и уведомляет о выходе
5. Когда все философы вышли, Table Service выводит итоговую статистику

## Просмотр результатов

Итоговая статистика выводится в логи Table Service после завершения работы всех философов. Также можно просмотреть логи через:

```bash
docker-compose logs table-service
```


