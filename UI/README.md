# MeetSpace - Frontend Setup Instructions

## Overview
The frontend is built using Angular 17 with Angular Material for UI components.

## Prerequisites
- Node.js 18.x or later (LTS recommended)
- npm 9.x or later
- Angular CLI 17

## Setup Steps

### 1. Install Global Dependencies
```bash
npm install -g @angular/cli
```

### 2. Install Project Dependencies
```bash
cd UI
npm install
```

### 3. Configure API Endpoint
Update the API endpoint in `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### 4. Start Development Server
```bash
ng serve
# or
npm start
```

The application will be available at `http://localhost:4200`

## Build for Production
```bash
ng build --configuration production
```

The build artifacts will be stored in the `dist/` directory.

## Running Tests
```bash
ng test
```

## Project Structure
```
UI/
├── src/
│   ├── app/
│   │   ├── core/              # Singleton services, guards, interceptors
│   │   ├── shared/            # Shared components
│   │   ├── features/          # Feature modules (Auth, Offices, Rooms, Bookings, Users)
│   │   ├── models/            # TypeScript interfaces
│   │   └── services/          # API services
│   ├── assets/                # Static assets
│   └── styles/                # Global styles
└── package.json
```

## Environment Setup
Create environment files if needed:
- `src/environments/environment.ts` (development)
- `src/environments/environment.prod.ts` (production)
