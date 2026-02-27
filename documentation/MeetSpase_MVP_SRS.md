# MeetSpase - Software Requirements Specification (SRS)

## MVP Version 1.0

------------------------------------------------------------------------

## 1. Introduction

### 1.1 Purpose

This document describes the functional and non-functional requirements
for the MVP version of **MeetSpase**, a web-based meeting room booking
system designed for organizations with multiple offices.

### 1.2 Scope

MeetSpase allows employees to: - Book meeting rooms in any office -
Manage meeting rooms (Manager/Admin) - Manage offices (Admin) - Manage
users (Admin) - Authenticate using JWT-based authorization

The system supports multiple offices, and users are not restricted to
any specific office when booking rooms.

------------------------------------------------------------------------

## 2. Overall Description

### 2.1 Product Perspective

MeetSpase is a web-based client-server application.

**Technology Stack:** - Backend: ASP.NET Core Web API - ORM: Entity
Framework Core - Database: Microsoft SQL Server - Authentication: JWT -
Frontend: Angular + Angular Material - Architecture Pattern: CQRS

------------------------------------------------------------------------

## 3. User Roles and Permissions

  Action               Employee   Manager   Admin
  -------------------- ---------- --------- -------
  View Offices         ✅         ✅        ✅
  View Rooms           ✅         ✅        ✅
  Create Booking       ✅         ✅        ✅
  Cancel Own Booking   ✅         ✅        ✅
  Cancel Any Booking   ❌         ✅        ✅
  Manage Rooms         ❌         ✅        ✅
  Manage Offices       ❌         ❌        ✅
  Manage Users         ❌         ❌        ✅

------------------------------------------------------------------------

## 4. Functional Requirements

### 4.1 Authentication

#### FR-1 Login

-   User provides Email and Password.
-   System validates credentials.
-   JWT token is generated containing:
    -   UserId
    -   Role
    -   Expiration

#### FR-2 Authorization

-   Role-based access control (RBAC).
-   API endpoints protected via policies.
-   Invalid or expired token results in authentication failure.

------------------------------------------------------------------------

### 4.2 Office Management (Admin Only)

#### FR-3 Create Office

Fields: - Name - Address - IsActive

#### FR-4 Update Office

#### FR-5 Deactivate Office

-   Office is not physically deleted.
-   Uses `IsActive` flag.

------------------------------------------------------------------------

### 4.3 Room Management (Manager and Admin)

#### FR-6 Create Room

Fields: - Name - Capacity - Description - OfficeId - IsActive

#### FR-7 Update Room

#### FR-8 Deactivate Room

------------------------------------------------------------------------

### 4.4 Booking Management (All Roles)

#### FR-9 View Offices

-   Only active offices are displayed.

#### FR-10 View Rooms

-   User selects an office.
-   System displays active rooms for that office.
-   Filtering options:
    -   Date
    -   Time interval
    -   Capacity

#### FR-11 Create Booking

User provides: - Office - Room - Date - StartTime - EndTime - Title

System must: - Ensure office is active - Ensure room is active -
Validate no time conflicts - Persist booking record

#### FR-12 Booking Conflict Rule

Booking creation must be rejected if:

(NewStart \< ExistingEnd) AND (NewEnd \> ExistingStart)

Validation occurs: - At Application Layer - At Database level (within
transaction)

#### FR-13 Cancel Booking

-   Users may cancel their own bookings.
-   Manager and Admin may cancel any booking.
-   Cancellation uses `IsCancelled = true`.

------------------------------------------------------------------------

### 4.5 User Management (Admin Only)

#### FR-14 Create User

Fields: - Name - Email - Password - Role - IsActive

#### FR-15 Update User

#### FR-16 Deactivate User

------------------------------------------------------------------------

## 5. Data Model

### 5.1 Office

-   Id
-   Name
-   Address
-   IsActive

### 5.2 Room

-   Id
-   Name
-   Capacity
-   Description
-   OfficeId (FK)
-   IsActive

### 5.3 User

-   Id
-   Name
-   Email
-   PasswordHash
-   Role (Admin / Manager / Employee)
-   IsActive

### 5.4 Booking

-   Id
-   RoomId (FK)
-   UserId (FK)
-   StartTime
-   EndTime
-   Title
-   CreatedAt
-   IsCancelled

------------------------------------------------------------------------

## 6. Business Rules

1.  Users are not assigned to any specific office.
2.  Users may book rooms in any active office.
3.  Booking conflict validation applies per room.
4.  Physical deletion is not allowed (soft delete only).
5.  Booking cannot be created in inactive rooms or offices.

------------------------------------------------------------------------

## 7. Non-Functional Requirements

### 7.1 Performance

-   API response time ≤ 500 ms under normal load.
-   Room availability check ≤ 1 second.

### 7.2 Security

-   Passwords must be hashed.
-   JWT expiration must be enforced.
-   HTTPS required.
-   Role-based authorization.
-   Input validation required.
-   Protection against SQL Injection and XSS.

### 7.3 Data Integrity

-   Transactions required during booking creation.
-   Index on (RoomId, StartTime, EndTime).

------------------------------------------------------------------------

## 8. MVP Limitations

The following features are NOT included in MVP:

-   Notifications
-   Booking approval workflows
-   Calendar integrations
-   Audit logging
-   Recurring bookings
-   Mobile application

------------------------------------------------------------------------

## 9. Architecture Overview

### 9.1 Backend Layers

-   API Layer
-   Application Layer (CQRS)
-   Domain Layer
-   Infrastructure Layer

### 9.2 CQRS Structure

**Commands:** - CreateBookingCommand - CancelBookingCommand -
CreateRoomCommand - UpdateRoomCommand - CreateUserCommand -
UpdateUserCommand - CreateOfficeCommand

**Queries:** - GetOfficesQuery - GetRoomsQuery -
GetAvailableRoomsQuery - GetBookingsQuery - GetUsersQuery

------------------------------------------------------------------------

## 10. MVP Acceptance Criteria

The MVP is considered complete when:

-   JWT authentication works correctly.
-   Role-based access restrictions are enforced.
-   Users can book rooms in any office.
-   Booking conflicts are prevented.
-   Admin can manage users and offices.
-   Manager can manage rooms.
-   Employee can create and cancel own bookings.

------------------------------------------------------------------------

End of Document
