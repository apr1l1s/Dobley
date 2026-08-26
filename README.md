# Dobley

Dobley представляет собой backend-систему на .NET для учёта продуктов в пользовательских хранилищах. Архитектура решения разделена на endpoint-, domain- и data-слои. Endpoint-проекты отвечают за HTTP-контракты, доменный слой содержит бизнес-правила и сценарии, data-слой инкапсулирует работу с базой данных, миграциями, репозиториями и инфраструктурными зависимостями.

## Структура проекта

```text
Dobley.Endpoints.Api       API продуктов и хранилищ, JWT-авторизация, Swagger
Dobley.Endpoints.Auth      Регистрация, вход, refresh-токены, выпуск JWT, Swagger
Dobley.Endpoints.Gateway   YARP gateway для маршрутизации запросов к Auth и API
Dobley.Endpoints.Ui        Веб-интерфейс для работы с хранилищами, продуктами и Telegram-уведомлениями
Dobley.Workers.Notifications Worker уведомлений о сроках годности и Telegram-интеграции
Dobley.Domain.Core         Сущности, валидация, формы, use cases, контракты репозиториев
Dobley.Data.Core           EF Core DbContext, репозитории, миграции, dependency injection
Dobley.Domain.Core.Tests   Тесты доменной валидации и opt-in smoke-тесты gateway
observability              Конфигурация OpenTelemetry Collector, datasource и Grafana dashboard
compose.yaml               Локальное окружение Gateway/API/Auth/UI/Worker/Postgres/Redis/RabbitMQ/Elastic/Grafana
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

NotificationDeliveries
- Id int primary key
- UserName varchar(100) foreign key -> Users.Login
- Channel varchar(50)
- Destination varchar(300)
- ProductId int foreign key -> Products.Id
- ExpirationDate timestamp
- Subject varchar(200)
- Body varchar(2000)
- DateAdded timestamp
- DateUpdated timestamp

NotificationOutboxMessages
- Id int primary key
- MessageId uuid unique
- Channel varchar(50)
- Destination varchar(300)
- Subject varchar(200)
- Body varchar(2000)
- AttemptCount int
- DateProcessed timestamp nullable
- Error varchar(2000) nullable
- DateAdded timestamp
- DateUpdated timestamp

NotificationInboxMessages
- Id int primary key
- MessageId uuid unique
- Channel varchar(50)
- Destination varchar(300)
- DateAdded timestamp
- DateUpdated timestamp
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
UI: http://localhost:5000/ui/
Postgres: localhost:5432
Redis: localhost:6379
RabbitMQ: localhost:5672
RabbitMQ Management: http://localhost:15672
Grafana: http://localhost:3000
Elasticsearch: http://localhost:9200
```

Auth и Product API являются внутренними Docker-сервисами. Клиентские запросы направляются через gateway.

Веб-интерфейс по адресу `http://localhost:5000/ui/` поддерживает регистрацию нового пользователя и вход в существующую учетную запись. После регистрации пользователь может сразу создавать хранилища, продукты и открывать Telegram-бота для получения ссылки на UI.

Маршруты gateway:

```text
/auth/* -> Dobley.Endpoints.Auth
/api/*  -> Dobley.Endpoints.Api
/ui/*   -> Dobley.Endpoints.Ui
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
GET /api/products/categories
GET /api/products/unit-types
GET /api/products/{id}
PUT /api/products/{id}
DELETE /api/products/{id}
POST /api/products/create
```

Admin database:

```text
GET /api/admin/db/tables
GET /api/admin/db/{tableName}
GET /api/admin/db/{tableName}/{key}
POST /api/admin/db/{tableName}
PATCH /api/admin/db/{tableName}/{key}
DELETE /api/admin/db/{tableName}/{key}
POST /api/admin/users
PATCH /api/admin/users/{login}/password
```

Admin database endpoints доступны в Swagger при включённом `ENABLE_SWAGGER=true` и требуют JWT. Через общий CRUD можно работать со всеми EF-таблицами, включая `Users`; специальные endpoints `/api/admin/users` дополнительно хешируют пароль пользователя.

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

## CI/CD

В репозитории настроены GitHub Actions:

```text
.github/workflows/ci.yml      проверка pull request и push в master
.github/workflows/docker.yml  публикация Docker images в GitHub Container Registry
```

В GitHub Actions эти процессы отображаются как `Проверка проекта` и `Публикация Docker images`.
Оба workflow поддерживают ручной запуск через кнопку `Run workflow`; при ручном запуске можно указать причину запуска.

CI выполняет:

```text
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet ef migrations script --idempotent
docker compose config --quiet
docker build для API, Auth, Gateway, UI и Notifications
проверку, что Telegram token не попал в репозиторий
```

На push в `master` Docker workflow публикует образы в `ghcr.io` с тегами `latest` и `sha-<commit>`.
Для публикации используется стандартный `GITHUB_TOKEN`, поэтому дополнительных secrets для GHCR не требуется.
Секреты приложения, например `TELEGRAM_BOT_TOKEN`, `SECRET_KEY`, `DB_PASSWORD` и `RABBITMQ_PASSWORD`, должны храниться в GitHub Secrets и передаваться только на этапе реального деплоя.

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
TELEGRAM_BOT_USERNAME       username Telegram-бота для открытия чата из UI; значение по умолчанию dobley_dev_bot
TELEGRAM_ALLOWED_CHAT_ID    Telegram chat id пользователя, которому бот отвечает в личных сообщениях
TELEGRAM_ALLOWED_USERNAME   Telegram username пользователя, которому бот отвечает в личных сообщениях
NOTIFICATION_CHANNEL        канал отправки напоминаний; сейчас доступен Telegram, архитектурно заложен Email
NOTIFICATION_DESTINATION    адрес доставки уведомлений; для Telegram это chat id
NOTIFICATION_USER_NAME      пользователь Dobley, по хранилищам которого worker ищет продукты для уведомлений
DOBLEY_UI_URL               публичная ссылка на UI, которую бот отправляет пользователю
DEFAULT_NOTIFY_BEFORE_DAYS  количество дней до уведомления о сроке годности
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
UI request logs: выдача веб-интерфейса, config.js и health endpoint
Notifications logs: watcher сроков годности, RabbitMQ-публикация и Telegram-отправка
Auth logs: регистрация, вход, refresh rotation, logout без паролей и токенов
API exception logs: предупреждения доменной валидации и необработанные ошибки
```

Grafana:

```text
URL: http://localhost:3000
Локальный логин по умолчанию: admin
Локальный пароль по умолчанию: admin
Dashboard: Dobley Observability
UI dashboard: Dobley UI
Database dashboard: Dobley Database
Service filter: "*" отображает Gateway, API, Auth, UI и Notifications вместе
Dashboard links: Gateway health, UI, Database dashboard, UI dashboard и Elasticsearch
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

Dashboard `Dobley Database` использует PostgreSQL datasource и показывает:

```text
Количество пользователей, хранилищ и продуктов
Продукты со сроком годности в ближайшие 3 дня
Состояние NotificationDeliveries, NotificationOutboxMessages и NotificationInboxMessages
Размер базы данных и количество подключений
CPU, память и файловая система Postgres-контейнера
Cache hit ratio, активные запросы, ожидания и locks
Количество живых строк по доменным таблицам
Список продуктов со сроком годности
Динамику добавления продуктов и хранилищ
Размеры таблиц и оценку live/dead rows
```

## Уведомления о сроке годности

Уведомления не завязаны на Telegram как на доменную модель. В домене используется общий канал `NotificationChannel`, адрес доставки `Destination`, факт отправки `NotificationDeliveries`, исходящая очередь БД `NotificationOutboxMessages` и входящая дедупликация `NotificationInboxMessages`. Telegram является только одной реализацией отправителя; для Email достаточно добавить новый sender под тот же интерфейс.

Telegram-бот больше не регистрирует чат, не создаёт коды подключения и не управляет подписками через команды. Он работает только в личных сообщениях конкретного разрешённого пользователя и отправляет ссылку на UI из `DOBLEY_UI_URL`. Доступ задаётся через `TELEGRAM_ALLOWED_CHAT_ID` или `TELEGRAM_ALLOWED_USERNAME`; если оба значения пустые, бот не отвечает на сообщения.

Для отправки напоминаний worker использует `NOTIFICATION_DESTINATION`; для обратной совместимости при пустом значении берётся `TELEGRAM_ALLOWED_CHAT_ID`. Продукты выбираются из хранилищ пользователя `NOTIFICATION_USER_NAME`, значение по умолчанию `admin`. Интервал предупреждения задаётся `DEFAULT_NOTIFY_BEFORE_DAYS`.

Сценарий подключения:

```text
1. Пользователь открывает Telegram-бота в личном чате.
2. Бот проверяет, что чат личный и пользователь разрешён настройками `TELEGRAM_ALLOWED_CHAT_ID` или `TELEGRAM_ALLOWED_USERNAME`.
3. Бот отправляет ссылку на UI из `DOBLEY_UI_URL`.
4. Expiration watcher ищет продукты пользователя `NOTIFICATION_USER_NAME` с ExpirationDate в пределах `DEFAULT_NOTIFY_BEFORE_DAYS`.
5. Worker сохраняет `NotificationDelivery` и `NotificationOutboxMessage`.
6. Outbox publisher читает необработанные сообщения из БД и публикует их в RabbitMQ.
7. Inbox consumer читает RabbitMQ, проверяет `NotificationInboxMessages` по `MessageId` и пропускает дубликаты.
8. Sender registry выбирает отправителя по `NotificationChannel`; для Telegram вызывается Telegram Bot API.
9. После успешной отправки inbox consumer сохраняет `NotificationInboxMessage`.
```

`Outbox` используется между расчётом уведомлений и RabbitMQ: если RabbitMQ временно недоступен, сообщение остаётся в `NotificationOutboxMessages` и будет опубликовано повторно. `Inbox` используется после RabbitMQ: если сообщение доставлено повторно, `NotificationInboxMessages` не даст отправить его пользователю второй раз.

`NotificationDeliveries` хранит бизнес-факт уведомления. Уникальность задаётся связкой `UserName + Channel + Destination + ProductId + ExpirationDate`, поэтому после рестарта worker не создаёт повторное уведомление по тому же продукту и той же дате срока годности.

Команды Telegram-бота:

```text
/start открыть ссылку на UI
/help  открыть ссылку на UI
```

Пример создания продукта со сроком годности:

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
