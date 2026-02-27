# Task 016 - Booking Conflict Rule

**Feature:** Booking Management (All Roles)

**Description:**

Booking creation must be rejected if:

```
(NewStart < ExistingEnd) AND (NewEnd > ExistingStart)
```

Validation occurs:
- At Application Layer
- At Database level (within transaction)

**Notes:**

This is FR-12 from the SRS.