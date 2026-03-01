import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BookingService } from '../../../core/services/booking.service';
import { RoomFilter, RoomListItem } from '../../../models/entities.model';

@Component({
  selector: 'app-select-room',
  templateUrl: './select-room.component.html',
  styleUrls: ['./select-room.component.scss'],
})
export class SelectRoomComponent implements OnInit {
  public readonly hours: number[] = Array.from({ length: 24 }, (_, i) => i);
  public readonly minutes: number[] = [0, 15, 30, 45];

  public rooms: RoomListItem[] = [];
  public isLoading: boolean = false;
  public errorMessage: string | null = null;
  public officeName: string = '';
  public filterForm: FormGroup;

  private officeId: string = '';

  constructor(
    private readonly bookingService: BookingService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: FormBuilder,
  ) {
    this.filterForm = this.fb.group({
      minCapacity: [null],
      date: [null],
      startHour: [null],
      startMinute: [null],
      endHour: [null],
      endMinute: [null],
    });
  }

  public ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.officeId = params['officeId'] ?? '';
      this.officeName = params['officeName'] ?? 'Office';
      if (!this.officeId) {
        this.router.navigate(['/book']);
        return;
      }
      this.loadRooms();
    });
  }

  public onSearch(): void {
    this.loadRooms();
  }

  public onClear(): void {
    this.filterForm.reset();
    this.loadRooms();
  }

  public goBack(): void {
    this.router.navigate(['/book']);
  }

  public bookRoom(room: RoomListItem): void {
    const v = this.filterForm.value;
    const dateVal: Date | null = v.date ? new Date(v.date) : null;

    this.router.navigate(['/book/create'], {
      queryParams: {
        officeId: this.officeId,
        officeName: this.officeName,
        roomId: room.id,
        roomName: room.name,
        ...(dateVal ? { date: dateVal.toISOString().substring(0, 10) } : {}),
        ...(v.startHour != null ? { startHour: v.startHour } : {}),
        ...(v.startMinute != null ? { startMinute: v.startMinute } : {}),
        ...(v.endHour != null ? { endHour: v.endHour } : {}),
        ...(v.endMinute != null ? { endMinute: v.endMinute } : {}),
      },
    });
  }

  public formatHour(h: number): string {
    return h.toString().padStart(2, '0') + ':00';
  }

  public formatMinute(m: number): string {
    return m.toString().padStart(2, '0');
  }

  private buildFilter(): RoomFilter {
    const v = this.filterForm.value;
    const dateVal: Date | null = v.date ? new Date(v.date) : null;

    const startTime: string | null =
      v.startHour != null && v.startMinute != null
        ? `${String(v.startHour).padStart(2, '0')}:${String(v.startMinute).padStart(2, '0')}`
        : null;

    const endTime: string | null =
      v.endHour != null && v.endMinute != null
        ? `${String(v.endHour).padStart(2, '0')}:${String(v.endMinute).padStart(2, '0')}`
        : null;

    return {
      officeId: this.officeId,
      minCapacity: v.minCapacity ?? null,
      date: dateVal ? dateVal.toISOString().substring(0, 10) : null,
      startTime,
      endTime,
    };
  }

  private loadRooms(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.bookingService.getRooms(this.buildFilter()).subscribe({
      next: (data: RoomListItem[]) => {
        this.rooms = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load rooms. Please try again.';
        this.isLoading = false;
      },
    });
  }
}

