## Task 016 - Booking Conflict Rule

**Feature:** Booking Management (All Roles)

**Description:**

Booking creation must be rejected if:

```text
(NewStart < ExistingEnd) AND (NewEnd > ExistingStart).
This room already booked by user or some one else did it for provided time range.
```

Validation occurs:
- At Application Layer
- At Database level (within transaction)

**Notes:**

This is FR-12 from the SRS.