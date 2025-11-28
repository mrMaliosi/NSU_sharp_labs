# Установка инструментов Entity Framework Core

Для работы с миграциями необходимо установить инструмент `dotnet-ef`.

## Вариант 1: Глобальная установка (рекомендуется)

Выполните в командной строке или PowerShell:

```bash
dotnet tool install --global dotnet-ef
```

После установки проверьте:
```bash
dotnet ef --version
```

## Вариант 2: Локальная установка (через файл конфигурации)

В проекте уже создан файл `.config/dotnet-tools.json`. Для восстановления локальных инструментов выполните:

```bash
cd DiningPhilosophers
dotnet tool restore
```

## Вариант 3: Установка конкретной версии

Если нужна конкретная версия, совместимая с EF Core 9.0:

```bash
dotnet tool install --global dotnet-ef --version 9.0.0
```

## После установки

После установки инструмента вы сможете выполнить команды миграций:

```bash
cd DiningPhilosophers
dotnet ef migrations add InitialCreate --project src/DiningPhilosophers.Core --startup-project src/DiningPhilosophers.App
dotnet ef database update --project src/DiningPhilosophers.Core --startup-project src/DiningPhilosophers.App
```

## Примечание

Если у вас проблемы с кириллицей в пути (Windows), попробуйте:
1. Открыть командную строку (cmd) вместо PowerShell
2. Или использовать полные пути без кириллицы
3. Или временно переименовать папки с кириллицей

