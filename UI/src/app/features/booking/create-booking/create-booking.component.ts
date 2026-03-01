import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BookingService } from '../../../core/services/booking.service';
import { CreateBookingRequest } from '../../../models/entities.model';

@Component({
  selector: 'app-create-booking',
  templateUrl: './create-booking.component.html',
  styleUrls: ['./create-booking.component.scss'],
})
export class CreateBookingComponent implements OnInit {
  public readonly hours: number[] = Array.from({ length: 24 }, (_, i) => i);
  public readonly minutes: number[] = [0, 15, 30, 45];

  public bookingForm: FormGroup;
  public isLoading: boolean = false;
  public errorMessage: string | null = null;

  public officeId: string = '';
  public officeName: string = '';
  public roomId: string = '';
  public roomName: string = '';

  public readonly minDate: Date = new Date();

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly bookingService: BookingService,
  ) {
    this.bookingForm = this.fb.group(
      {
        date: [null, Validators.required],
        startHour: [9, Validators.required],
        startMinute: [0, Validators.required],
        endHour: [10, Validators.required],
        endMinute: [0, Validators.required],
        title: ['', [Validators.required, Validators.maxLength(200)]],
      },
      { validators: this.endAfterStartValidator },
    );
  }

  public ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.officeId = params['officeId'] ?? '';
      this.roomId = params['roomId'] ?? '';
      this.officeName = params['officeName'] ?? '';
      this.roomName = params['roomName'] ?? '';

      if (!this.officeId || !this.roomId) {
        this.router.navigate(['/book']);
        return;
      }

      const patch: Record<string, unknown> = {};
      if (params['date']) {
        patch['date'] = new Date(params['date']);
      }
      if (params['startHour'] != null) {
        patch['startHour'] = +params['startHour'];
      }
      if (params['startMinute'] != null) {
        patch['startMinute'] = +params['startMinute'];
      }
      if (params['endHour'] != null) {
        patch['endHour'] = +params['endHour'];
      }
      if (params['endMinute'] != null) {
        patch['endMinute'] = +params['endMinute'];
      }
      if (Object.keys(patch).length > 0) {
        this.bookingForm.patchValue(patch);
      }
    });
  }

  public get date(): AbstractControl {
    return this.bookingForm.get('date')!;
  }

  public get title(): AbstractControl {
    return this.bookingForm.get('title')!;
  }

  public formatHour(h: number): string {
    return h.toString().padStart(2, '0') + ':00';
  }

  public formatMinute(m: number): string {
    return m.toString().padStart(2, '0');
  }

  public onSubmit(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const v = this.bookingForm.value;
    const date: Date = v.date as Date;
    const dateStr: string = date.toISOString().substring(0, 10);
    const startTimeStr: string = `${String(v.startHour).padStart(2, '0')}:${String(v.startMinute).padStart(2, '0')}:00`;
    const endTimeStr: string = `${String(v.endHour).padStart(2, '0')}:${String(v.endMinute).padStart(2, '0')}:00`;

    const request: CreateBookingRequest = {
      officeId: this.officeId,
      roomId: this.roomId,
      date: dateStr,
      startTime: startTimeStr,
      endTime: endTimeStr,
      title: v.title as string,
    };

    this.isLoading = true;
    this.errorMessage = null;

    this.bookingService.createBooking(request).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/my-bookings']);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        if (err.status === 409) {
          this.errorMessage = 'This room is already booked for the selected time slot. Please choose a different time.';
        } else if (err.status === 400) {
          this.errorMessage = err.error?.message ?? 'Invalid booking details. Please check your input.';
        } else {
          this.errorMessage = 'Failed to create booking. Please try again.';
        }
      },
    });
  }

  public goBack(): void {
    this.router.navigate(['/book/rooms'], {
      queryParams: { officeId: this.officeId, officeName: this.officeName },
    });
  }

  private endAfterStartValidator(group: AbstractControl): { [key: string]: boolean } | null {
    const endHour: number = group.get('endHour')?.value as number;
    const endMinute: number = group.get('endMinute')?.value as number;
    const startHour: number = group.get('startHour')?.value as number;
    const startMinute: number = group.get('startMinute')?.value as number;

    if (endHour == null || endMinute == null || startHour == null || startMinute == null) {
      return null;
    }

    const startTotal: number = startHour * 60 + startMinute;
    const endTotal: number = endHour * 60 + endMinute;

    return endTotal <= startTotal ? { endNotAfterStart: true } : null;
  }
}
