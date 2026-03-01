import { Component, OnInit } from '@angular/core';
import { BookingService } from '../../core/services/booking.service';
import { BookingListItem } from '../../models/entities.model';

@Component({
  selector: 'app-my-booking',
  templateUrl: './my-booking.component.html',
  styleUrls: ['./my-booking.component.scss'],
})
export class MyBookingComponent implements OnInit {
  public readonly displayedColumns: string[] = ['title', 'office', 'room', 'date', 'time', 'status', 'actions'];

  public bookings: BookingListItem[] = [];
  public isLoading: boolean = false;
  public errorMessage: string | null = null;
  public cancellingId: string | null = null;
  public confirmCancelId: string | null = null;

  constructor(private readonly bookingService: BookingService) {}

  public ngOnInit(): void {
    this.loadBookings();
  }

  public onCancelClick(id: string): void {
    this.confirmCancelId = id;
  }

  public onCancelConfirm(): void {
    if (!this.confirmCancelId) {
      return;
    }
    const id: string = this.confirmCancelId;
    this.confirmCancelId = null;
    this.cancellingId = id;
    this.errorMessage = null;

    this.bookingService.cancelBooking(id).subscribe({
      next: () => {
        this.cancellingId = null;
        this.loadBookings();
      },
      error: () => {
        this.cancellingId = null;
        this.errorMessage = 'Failed to cancel booking. Please try again.';
      },
    });
  }

  public onCancelDismiss(): void {
    this.confirmCancelId = null;
  }

  public getTimeRange(booking: BookingListItem): string {
    return `${booking.startTime} – ${booking.endTime}`;
  }

  private loadBookings(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.bookingService.getUserBookings().subscribe({
      next: (data: BookingListItem[]) => {
        this.bookings = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load bookings. Please try again.';
        this.isLoading = false;
      },
    });
  }
}

