## Solution Structure

## Project Layout

```text
MeetSpace/
├── Backend/
│   ├── MeetSpace.API/           # ASP.NET Core Web API entry point
│   │   ├── Controllers/         # API endpoints
│   │   ├── Middleware/          # Custom middleware
│   │   ├── Program.cs           # Application bootstrap
│   │   ├── appsettings.json     # Configuration
│   │   └── MeetSpace.API.csproj
│   ├── MeetSpace.Domain/        # Domain entities and interfaces
│   │   ├── Entities/            # Domain models
│   │   ├── Interfaces/          # Repository/service contracts
│   │   └── MeetSpace.Domain.csproj
│   ├── MeetSpace.Application/   # CQRS commands and queries
│   │   ├── Commands/            # CQRS commands
│   │   ├── Queries/             # CQRS queries
│   │   ├── DTOs/                # Data transfer objects
│   │   ├── Validators/          # FluentValidation validators
│   │   └── MeetSpace.Application.csproj
│   ├── MeetSpace.Infrastructure/# Data access and repositories
│   │   ├── Data/                # DbContext and migrations
│   │   ├── Repositories/        # Repository implementations
│   │   └── MeetSpace.Infrastructure.csproj
│   └── README.md
├── UI/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/            # Guards, interceptors, singleton services
│   │   │   ├── shared/          # Shared components
│   │   │   ├── features/        # Feature modules
│   │   │   ├── models/          # TypeScript interfaces
│   │   │   └── services/        # API communication
│   │   ├── assets/              # Images, icons
│   │   ├── styles/              # Global styles
│   │   ├── main.ts              # Bootstrap
│   │   └── index.html
│   ├── package.json
│   ├── angular.json
│   ├── tsconfig.json
│   └── README.md
├── documentation/
│   ├── MeetSpace_MVP_SRS.md     # Project requirements
│   ├── tasks/                   # Task breakdown (000-017)
│   └── README.md
└── prompts/
    ├── dotnet-backend-expert.md
    └── angular-frontend-expert.md
```

## Technology Stack Overview

### Backend
- **.NET 8** - Runtime
- **ASP.NET Core Web API** - API framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **MediatR** - CQRS pattern
- **FluentValidation** - Validation framework
- **Swagger/OpenAPI** - API documentation
- **JWT** - Authentication

### Frontend
- **Angular 17** - Frontend framework
- **TypeScript** - Language
- **Angular Material** - UI components
- **RxJS** - Reactive programming
- **HttpClient** - HTTP communication

## Key Features

1. **Authentication & Authorization**
   - JWT token-based authentication
   - Role-based access control (Admin, Manager, Employee)

2. **Office Management**
   - Create, read, update offices
   - Soft delete (IsActive flag)

3. **Room Management**
   - Create, read, update rooms
   - Associate rooms with offices

4. **Booking System**
   - Create, view, cancel bookings
   - Conflict detection and validation

5. **User Management**
   - Manage system users
   - Role assignment

## Getting Started

### Backend Development
See [Backend README](./Backend/README.md)

### Frontend Development
See [UI README](./UI/README.md)

## Development Workflow

1. Read the required task from `documentation/tasks/`
2. Reference the MVP SRS in `documentation/MeetSpace_MVP_SRS.md`
3. Use the appropriate expert prompt:
   - Backend: `prompts/dotnet-backend-expert.md`
   - Frontend: `prompts/angular-frontend-expert.md`
4. Implement the task following the architecture guidelines

## Next Steps

- Task 001: Implement Login endpoint
- Task 002: Implement Authorization policies
- Task 003: Implement Create User endpoint
- ... and so on
