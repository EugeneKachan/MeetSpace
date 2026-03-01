## MeetSpace - Backend Setup Instructions

## Overview
The backend is structured using a layered architecture with the following projects:
- **MeetSpace.API**: ASP.NET Core Web API (entry point)
- **MeetSpace.Domain**: Domain entities and interfaces
- **MeetSpace.Application**: CQRS commands, queries, and DTOs
- **MeetSpace.Infrastructure**: Data access and repositories

## Prerequisites
- .NET 8 SDK or later
- SQL Server (LocalDB or full installation)

## Setup Steps

### 1. Restore Dependencies
```bash
cd Backend
dotnet restore
```

### 2. Configure Database Connection
Update the connection string in `MeetSpace.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=MeetSpace;Trusted_Connection=true;"
}
```

### 3. Apply Data Migrations
```bash
cd Backend/MeetSpace.API
dotnet ef database update --project ../MeetSpace.Infrastructure
```

### 4. Run the Application
```bash
dotnet run
```

The API will be available at `https://localhost:5000`

## JWT Configuration
Configure JWT settings in `appsettings.json`. In production, use proper secret management (Azure Key Vault, etc.).

## API Documentation
Swagger/OpenAPI documentation will be available at `https://localhost:5000/swagger`
