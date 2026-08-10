# Dobley

Dobley - небольшой backend на .NET для учёта продуктов в пользовательских хранилищах. Решение разделено на endpoint, domain и data проекты: API остаётся тонким, а бизнес-правила живут в доменных сущностях и use case'ах.

## Структура проекта

```text
Dobley.Endpoints.Api       API продуктов и хранилищ, JWT-авторизация, Swagger
Dobley.Endpoints.Auth      Регистрация, вход, refresh-токены, выпуск JWT, Swagger
Dobley.Endpoints.Gateway   YARP gateway для маршрутизации Auth/API
Dobley.Domain.Core         Сущности, валидация, формы, use cases, контракты репозиториев
Dobley.Data.Core           EF Core DbContext, репозитории, миграции, dependency injection
Dobley.Domain.Core.Tests   Тесты доменной валидации и opt-in smoke-тесты gateway
compose.yaml               Локальное окружение Gateway/API/Auth/Postgres/Redis/Elastic/Grafana
```

## Структура БД

Текущая EF Core модель:

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
- StorageId int foreign key -> Storages.Id
- DateAdded timestamp
- DateUpdated timestamp
- DateDeleted timestamp nullable
```

Миграции лежат в `Dobley.Data.Core/Migrations`.

## Локальный запуск

Создай `.env` на основе `.env.example`, затем запусти:

```powershell
docker compose up --build
```

Значения в `.env.example` подходят только для локальной разработки. Перед использованием вне локального демо поменяй `SECRET_KEY`, доступы к базе и пароль Grafana.

Адреса по умолчанию:

```text
Gateway: http://localhost:5000
Gateway health: http://localhost:5000/health
Postgres: localhost:5432
Redis: localhost:6379
Grafana: http://localhost:3000
Elasticsearch: http://localhost:9200
```

Auth и Product API доступны как внутренние Docker-сервисы. Клиентские запросы нужно отправлять через gateway.

Маршруты gateway:

```text
/auth/* -> Dobley.Endpoints.Auth
/api/*  -> Dobley.Endpoints.Api
```

## Демо-сценарий

Можно использовать Postman collection из `postman/Dobley.postman_collection.json` или отправлять такие же запросы вручную через gateway.

1. Запустить stack:

```powershell
docker compose up --build
```

2. Проверить health endpoints:

```http
GET http://127.0.0.1:5000/health
GET http://127.0.0.1:3000/api/health
GET http://127.0.0.1:9200/_cluster/health
```

3. Зарегистрироваться и войти:

```http
POST http://127.0.0.1:5000/auth/reg
Content-Type: application/json

{
  "login": "demo-user",
  "password": "password"
}
```

```http
POST http://127.0.0.1:5000/auth/login
Content-Type: application/json

{
  "login": "demo-user",
  "password": "password"
}
```

4. Создать хранилище и продукт с полученным bearer token:

```http
POST http://127.0.0.1:5000/api/storages/create
Authorization: Bearer <access-token>
Content-Type: application/json

{
  "name": "Fridge",
  "description": "Kitchen fridge"
}
```

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

5. Спровоцировать ошибку валидации и посмотреть её в Grafana:

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

API вернёт русское сообщение об ошибке, а запрос появится в Grafana как `400` log record.

## Примеры запросов

Регистрация и вход:

```http
POST /auth/reg
Content-Type: application/json

{
  "login": "demo",
  "password": "password"
}
```

```http
POST /auth/login
Content-Type: application/json

{
  "login": "demo",
  "password": "password"
}
```

Обновление access token:

```http
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "<refresh-token>"
}
```

Выход:

```http
POST /auth/logout
Content-Type: application/json

{
  "refreshToken": "<refresh-token>"
}
```

Создание хранилища и продукта:

```http
POST /api/storages/create
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Fridge",
  "description": "Kitchen fridge"
}
```

```http
POST /api/products/create
Authorization: Bearer <token>
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

Методы продуктов и хранилищ работают только с данными текущего JWT-пользователя:

```text
GET /api/storages
GET /api/storages/{id}
PUT /api/storages/{id}
DELETE /api/storages/{id}
POST /api/storages/create
GET /api/products
GET /api/products/{id}
PUT /api/products/{id}
DELETE /api/products/{id}
POST /api/products/create
```

## Проверки для разработки

```powershell
dotnet build Dobley.sln
dotnet test Dobley.sln
dotnet ef database update --project Dobley.Data.Core
```

Gateway smoke-тесты выключены по умолчанию, потому что требуют запущенный Docker stack:

```powershell
$env:DOBLEY_RUN_GATEWAY_TESTS='true'
dotnet test Dobley.sln --filter FullyQualifiedName~GatewaySmokeTests
```

Они проверяют:

```text
Gateway health возвращает 200
Коллекция продуктов без токена возвращает 401
Создание продукта с некорректным телом возвращает русский 400 response
```

`SECRET_KEY` обязателен при запуске и должен содержать минимум 32 байта. `JWT_ISSUER` и `JWT_AUDIENCE` по умолчанию равны `apr1l1s_auth` и `apr1l1s_services`. `REDIS_CONNECTION` используется для хранения refresh-токенов в Redis; если переменная пустая, приложение использует in-memory cache. `SEED_DEV_DATA=true` включает локальные демо-данные: пользователь `demo` с паролем `password`, одно хранилище и один продукт. По умолчанию сидинг выключен.

## Логирование и наблюдаемость

Сервисы используют стандартный `ILogger` и OpenTelemetry:

```text
Центральные логи: OpenTelemetry Collector -> Elasticsearch
UI логов: Grafana на http://localhost:3000
Локальный fallback: Docker volumes с daily rolling log files для каждого сервиса
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
Service filter: "*" показывает Gateway, API и Auth вместе
Search filter: по умолчанию "Body:*"; примеры: Body:"products", Attributes.Path:"/api/products/"
Refresh: 5 секунд; Elasticsearch может отставать на несколько секунд после запроса
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

Переменные окружения для логирования:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://dobley.otel-collector:4317
LOG_FILE_PATH=/app/logs/<service>.log
```

Локальные fallback logs пишутся в Docker volumes как daily rolling files, например `/app/logs/api-YYYYMMDD.log`.
