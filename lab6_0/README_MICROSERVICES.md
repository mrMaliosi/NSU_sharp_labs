# Микросервисная архитектура "Обедающие философы"

Проект переписан на микросервисную архитектуру, где каждый философ и стол являются отдельными HTTP-сервисами, взаимодействующими через REST API.

## Архитектура

### Сервисы

1. **TableService** - центральный сервис, управляющий состоянием вилок и сбором метрик
   - Порт: 8080
   - Эндпоинты:
     - `GET /health` - проверка здоровья сервиса
     - `POST /api/philosophers/register` - регистрация философа
     - `POST /api/philosophers/{id}/exit` - уведомление о выходе философа
     - `POST /api/forks/{id}/acquire` - захват вилки
     - `POST /api/forks/{id}/release` - освобождение вилки
     - `GET /api/forks/{id}/state` - состояние вилки
     - `GET /api/forks` - состояние всех вилок
     - `POST /api/metrics/meal` - запись приема пищи
     - `POST /api/metrics/waiting-time` - обновление времени ожидания
     - `GET /api/metrics` - получение метрик

2. **PhilosopherService** - отдельный экземпляр для каждого философа (5 сервисов)
   - Порты: 8081-8085
   - Эндпоинты:
     - `GET /health` - проверка здоровья сервиса
     - `GET /api/philosopher/state` - состояние философа

## Запуск через Docker Compose

1. Убедитесь, что Docker и Docker Compose установлены

2. Запустите всю систему:
```bash
docker-compose up --build
```

3. Система автоматически:
   - Запустит TableService
   - Дождется готовности TableService (health check)
   - Запустит 5 экземпляров PhilosopherService
   - Каждый философ отработает настроенное количество минут (по умолчанию 5 минут)
   - После выхода всех философов TableService распечатает итоговые метрики

## Конфигурация

Конфигурация выполняется через переменные окружения в `docker-compose.yml`:

- `PHILOSOPHERS_COUNT` - количество философов (для TableService)
- `PHILOSOPHER_NAME` - имя философа
- `PHILOSOPHER_ID` - уникальный идентификатор философа
- `LEFT_FORK_ID` - ID левой вилки
- `RIGHT_FORK_ID` - ID правой вилки
- `TABLE_SERVICE_URL` - URL TableService
- `SIMULATION_DURATION_MINUTES` - длительность симуляции в минутах

## Структура проекта

```
.
├── TableService/              # Сервис стола
│   ├── Controllers/          # REST API контроллеры
│   ├── Services/            # Бизнес-логика
│   ├── Models/              # DTO модели
│   ├── Dockerfile
│   └── TableService.csproj
├── PhilosopherService/       # Сервис философа
│   ├── Controllers/         # REST API контроллеры
│   ├── Services/           # Бизнес-логика философа
│   ├── Models/             # DTO модели
│   ├── Dockerfile
│   └── PhilosopherService.csproj
├── DiningPhilosophers/       # Исходный проект (используется как библиотека)
└── docker-compose.yml       # Конфигурация Docker Compose
```

## Взаимодействие между сервисами

1. Философ регистрируется в TableService при запуске
2. Философ запрашивает вилки через REST API
3. TableService управляет состоянием вилок и метриками
4. Философ уведомляет TableService о приеме пищи
5. После завершения работы философ уведомляет TableService о выходе
6. TableService печатает итоговые метрики после выхода всех философов

