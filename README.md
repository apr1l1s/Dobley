# Dobley

Dobley is a small .NET backend for tracking products in user storages. The solution is split into endpoint, domain, and data projects so the API layer stays thin and business rules live in the domain model/use cases.

## Project Structure

```text
Dobley.Endpoints.Api       Product API, JWT authorization, Swagger
Dobley.Endpoints.Auth      Login/registration API, JWT issuing, Swagger
Dobley.Domain.Core         Entities, validation, forms, use cases, repository contracts
Dobley.Data.Core           EF Core DbContext, repositories, migrations, dependency injection
Dobley.Domain.Core.Tests   Domain validation tests
compose.yaml               Local API/Auth/Postgres environment
```

## Database Structure

Current EF Core model:

```text
Users
- Login varchar(100) primary key
- Password varchar(255)

Storages
- Id int primary key
- UserName varchar(100) foreign key -> Users.Login
- Name varchar(100)
- Description varchar(200)

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
```

Migrations are stored in `Dobley.Data.Core/Migrations`.

## Local Run

Create `.env` from `.env.example`, then run:

```powershell
docker compose up --build
```

Default endpoints:

```text
Auth API: http://localhost:5002
Product API: http://localhost:5001
Auth Swagger: http://localhost:5002/swagger
Product Swagger: http://localhost:5001/swagger
Postgres: localhost:5432
```

## Example Requests

Register and login:

```http
POST /reg
Content-Type: application/json

{
  "login": "demo",
  "password": "password"
}
```

```http
POST /login
Content-Type: application/json

{
  "login": "demo",
  "password": "password"
}
```

Create product with a bearer token:

```http
POST /products/create
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

## Development Checks

```powershell
dotnet build Dobley.sln
dotnet test Dobley.sln
dotnet ef database update --project Dobley.Data.Core
```

`SECRET_KEY` is required at runtime and must contain at least 32 bytes.
