# Dobley

Dobley представляет собой backend-систему на .NET для учёта продуктов в пользовательских хранилищах. Архитектура решения разделена на endpoint-, domain- и data-слои. Endpoint-проекты отвечают за HTTP-контракты, доменный слой содержит бизнес-правила и сценарии, data-слой инкапсулирует работу с базой данных, миграциями, репозиториями и инфраструктурными зависимостями.

## Структура проекта

```text
Dobley.Endpoints.Api       API продуктов и хранилищ, JWT-авторизация, Swagger
Dobley.Endpoints.Auth      Регистрация, вход, refresh-токены, выпуск JWT, Swagger
Dobley.Endpoints.Gateway   YARP gateway для маршрутизации запросов к Auth и API
Dobley.Domain.Core         Сущности, валидация, формы, use cases, контракты репозиториев
Dobley.Data.Core           EF Core DbContext, репозитории, миграции, dependency injection
Dobley.Domain.Core.Tests   Тесты доменной валидации и opt-in smoke-тесты gateway
compose.yaml               Локальное окружение Gateway/API/Auth/Postgres/Redis/Elastic/Grafana
```

## Структура базы данных

Текущая EF Core модель содержит следующие таблицы:

```text
Users
- Login varchar(100) primary key
- Password varchar(255)
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable

Storages
- Id int primary key
- UserName varchar(100) foreign key -> Users.Login
- Name varchar(100)
- Description varchar(200)
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable

Products
- Id int primary key
- Name varchar(100)
- Description varchar(200)
- Price decimal(18,2)
- Category varchar(100)
- Unit decimal(18,2)
- UnitType varchar(50)
- Barcode varchar(50)
- ExpirationDate timestamp nullable
- StorageId int foreign key -> Storages.Id
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable

NotificationRecipients
- Id int primary key
- UserName varchar(100) foreign key -> Users.Login
- Channel varchar(50)
- ExternalId varchar(200)
- DisplayName varchar(200) nullable
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable

NotificationInvites
- Id int primary key
- UserName varchar(100) foreign key -> Users.Login
- Code varchar(100)
- ExpiresAt timestamp
- UsedAt timestamp nullable
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable

StorageNotificationSubscriptions
- Id int primary key
- NotificationRecipientId int foreign key -> NotificationRecipients.Id
- StorageId int foreign key -> Storages.Id
- NotifyBeforeDays int
- IsEnabled boolean
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable
```

Миграции расположены в каталоге `Dobley.Data.Core/Migrations`.

## Требования для локального запуска

Для локального запуска требуется:

```text
Docker
Docker Compose
.NET SDK 9.0 для локальной сборки и запуска тестов без контейнеров
```

Конфигурация окружения задаётся через файл `.env`, формируемый на основе `.env.example`.

```powershell
docker compose up --build
```

Значения из `.env.example` предназначены для локальной разработки и демонстрационного запуска. Для сред, отличных от локального окружения, требуется переопределение `SECRET_KEY`, реквизитов базы данных и учётных данных Grafana.

Адреса сервисов по умолчанию:

```text
Gateway: http://localhost:5000
Gateway health: http://localhost:5000/health
Postgres: localhost:5432
Redis: localhost:6379
RabbitMQ: localhost:5672
RabbitMQ Management: http://localhost:15672
Grafana: http://localhost:3000
Elasticsearch: http://localhost:9200
```

Auth и Product API являются внутренними Docker-сервисами. Клиентские запросы направляются через gateway.

Маршруты gateway:

```text
/auth/* -> Dobley.Endpoints.Auth
/api/*  -> Dobley.Endpoints.Api
```

## Демонстрационный сценарий

Для проверки API может использоваться Postman collection из `postman/Dobley.postman_collection.json`. Эквивалентные HTTP-запросы могут быть выполнены вручную через gateway.

1. Запуск контейнерного окружения:

```powershell
docker compose up --build
```

2. Проверка health endpoints:

```http
GET http://127.0.0.1:5000/health
GET http://127.0.0.1:3000/api/health
GET http://127.0.0.1:9200/_cluster/health
```

3. Регистрация пользователя:

```http
POST http://127.0.0.1:5000/auth/reg
Content-Type: application/json

{
  "login": "demo-user",
  "password": "password"
}
```

4. Получение пары access/refresh token:

```http
POST http://127.0.0.1:5000/auth/login
Content-Type: application/json

{
  "login": "demo-user",
  "password": "password"
}
```

5. Создание хранилища:

```http
POST http://127.0.0.1:5000/api/storages/create
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "name": "Fridge",
  "description": "Kitchen fridge"
}
```

6. Создание продукта:

```http
POST http://127.0.0.1:5000/api/products/create
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "name": "Milk",
  "description": "Fresh milk",
  "price": 120,
  "category": "Dairy",
  "unit": 1,
  "unitType": "Liters",
  "barcode": "4600000000000",
  "storageId": 1
}
```

7. Пример запроса с ошибкой валидации:

```http
POST http://127.0.0.1:5000/api/products/create
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "name": "Bread",
  "description": "Good bread",
  "price": 78,
  "category": "Unknown",
  "unit": 1,
  "unitType": "Pieces",
  "barcode": "75623"
}
```

В случае некорректного тела запроса API возвращает `400` с русским сообщением об ошибке. Соответствующая запись также попадает в централизованное логирование и отображается в Grafana.

## Основные HTTP endpoints

Auth:

```text
POST /auth/reg
POST /auth/login
POST /auth/refresh
POST /auth/logout
```

Storages:

```text
GET /api/storages
GET /api/storages/{id}
PUT /api/storages/{id}
DELETE /api/storages/{id}
POST /api/storages/create
```

Products:

```text
GET /api/products
GET /api/products/{id}
PUT /api/products/{id}
DELETE /api/products/{id}
POST /api/products/create
```

Notifications:

```text
POST /api/notifications/invites/create
GET /api/notifications/recipients
POST /api/notifications/recipients/{recipientId}/subscriptions
```

Методы продуктов и хранилищ возвращают и изменяют только данные, принадлежащие текущему JWT-пользователю.

## Проверки разработки

Сборка решения:

```powershell
dotnet build Dobley.sln
```

Запуск тестов:

```powershell
dotnet test Dobley.sln
```

Применение миграций:

```powershell
dotnet ef database update --project Dobley.Data.Core
```

Gateway smoke-тесты выключены по умолчанию, поскольку требуют запущенного Docker stack:

```powershell
$env:DOBLEY_RUN_GATEWAY_TESTS='true'
dotnet test Dobley.sln --filter FullyQualifiedName~GatewaySmokeTests
```

Smoke-тесты проверяют:

```text
Gateway health возвращает 200
Коллекция продуктов без токена возвращает 401
Создание продукта с некорректным телом возвращает русский 400 response
```

## Конфигурация

Ключевые переменные окружения:

```text
SECRET_KEY                  секрет JWT, минимум 32 байта
JWT_ISSUER                  issuer JWT, значение по умолчанию apr1l1s_auth
JWT_AUDIENCE                audience JWT, значение по умолчанию apr1l1s_services
REDIS_CONNECTION            подключение к Redis для refresh-токенов
RABBITMQ_USER               пользователь RabbitMQ
RABBITMQ_PASSWORD           пароль RabbitMQ
RABBITMQ_NOTIFICATION_QUEUE очередь уведомлений о сроке годности
TELEGRAM_BOT_TOKEN          токен Telegram Bot API; хранится только в локальном .env
DEFAULT_NOTIFY_BEFORE_DAYS  количество дней до уведомления при подписке через bot invite
EXPIRATION_WATCH_INTERVAL_SECONDS интервал проверки сроков годности worker-сервисом
SEED_DEV_DATA               включение локальных демо-данных
OTEL_EXPORTER_OTLP_ENDPOINT endpoint OpenTelemetry Collector
LOG_FILE_PATH               путь к базовому локальному fallback log file для NLog
```

При пустом `REDIS_CONNECTION` приложение использует in-memory cache. Режим `SEED_DEV_DATA=true` создаёт локальные демонстрационные данные: пользователя `demo` с паролем `password`, одно хранилище и один продукт. По умолчанию сидинг выключен.

## Логирование и наблюдаемость

Сервисы используют стандартный `ILogger`, NLog и OpenTelemetry.

```text
Центральные логи: OpenTelemetry Collector -> Elasticsearch
UI логов: Grafana на http://localhost:3000
Локальный fallback: NLog пишет daily log files в Docker volumes для каждого сервиса
Gateway/API/Auth request logs: method, path, status code, elapsed time, trace id
Auth logs: регистрация, вход, refresh rotation, logout без паролей и токенов
API exception logs: предупреждения доменной валидации и необработанные ошибки
```

Grafana:

```text
URL: http://localhost:3000
Локальный логин по умолчанию: admin
Локальный пароль по умолчанию: admin
Dashboard: Dobley Observability
Service filter: "*" отображает Gateway, API и Auth вместе
Search filter: по умолчанию "Body:*"; примеры: Body:"products", Attributes.Path:"/api/products/"
Refresh: 5 секунд; Elasticsearch может отображать логи с задержкой в несколько секунд
```

Dashboard содержит:

```text
Logs / Warnings / Errors / 4xx counters
Log Rate By Service
HTTP Statuses
Recent Logs
Warnings, Errors, 4xx, 5xx
Raw Documents
```

Локальные fallback logs сохраняются в Docker volumes через NLog как daily files, например `/app/logs/api-2026-08-11.log`.

## ����������� � ����� ��������

��� ����������� ������������ ����������� ������ �����������. � ���� ��� ������ � ��������� Telegram: Telegram �������� ��������� ������ `NotificationChannel`, � ������� ������������� �������� � `NotificationRecipients.ExternalId`.

�������� �����������:

```text
1. ������������ Dobley ������ ��� ����������� ����� POST /api/notifications/invites/create.
2. ������� ��������� Telegram-���� � ���������� /start <code>.
3. Worker ������ NotificationRecipient � Channel=Telegram � ExternalId=chatId.
4. ���������� ������������� ������������� �� ������� ��������� ������������.
5. Expiration watcher ���� �������� � ExpirationDate � �������� NotifyBeforeDays.
6. Worker ��������� ��������� � RabbitMQ.
7. Telegram consumer ������ ������� � ���������� ��������� ����� Telegram Bot API.
```

������ �������� �������� �� ������ ��������:

```json
{
  "name": "Milk",
  "description": "Fresh milk",
  "price": 120,
  "category": "Dairy",
  "unit": 1,
  "unitType": "Liters",
  "barcode": "4600000000000",
  "expirationDate": "2026-08-15T00:00:00Z",
  "storageId": 1
}
```
