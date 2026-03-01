## Task 006 - Offices Page

**Feature:** Office Management (UI)

**Status:** ✅ Implemented (February 27, 2026)

**Description:**

- Implement the UI page displaying a list of offices in a table.
- Columns: Name, Address, Managers, Status, and actions.
- From this page Admins can add, deactivate, or edit offices; OfficeManagers see only their assigned offices.
- Editing or creating an office should open a modal window.
- Within the modal, also allow management of the office's rooms (add/remove/edit).

## Role-Based Behaviour

| Capability                      | Admin | OfficeManager |
|---------------------------------|-------|---------------|
| See all offices                 | ✅    | ❌ (assigned only) |
| Add office                      | ✅    | ❌            |
| Edit office details             | ✅    | ❌ (read-only) |
| Deactivate office               | ✅    | ❌            |
| Assign / remove managers        | ✅    | ❌            |
| Add / edit / deactivate rooms   | ✅    | ✅            |

## Office Manager Assignment (Enhancement)

Admins can assign one or more **OfficeManager** users to an office from within the edit dialog.

- Backend: `OfficeAssignment` join table (`OfficeId` + `UserId` composite PK, cascade deletes).
- API: `POST /api/offices/{id}/managers` (assign), `DELETE /api/offices/{id}/managers/{userId}` (remove).
- `GET /api/offices` automatically filters by the authenticated user when role is `OfficeManager`.

## Implementation Notes

- Angular 17 block control flow (`@if` / `@for`) used throughout the template to avoid Ivy compiler naming conflicts with `*matCellDef` sub-templates.
- `OfficesModule` imports `MatSelectModule` for the manager assignment dropdown.
- `OfficeDialogComponent` injects `UsersService` and `AuthService`; available managers list is filtered to active OfficeManager users not already assigned.

**Notes:**

Detailed behavior follows the SRS section 4.2 (Office Management) and 4.3 (Room Management) where applicable.