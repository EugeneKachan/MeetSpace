import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { BookingService } from '../../../core/services/booking.service';
import { ActiveOffice } from '../../../models/entities.model';

@Component({
  selector: 'app-select-office',
  templateUrl: './select-office.component.html',
  styleUrls: ['./select-office.component.scss'],
})
export class SelectOfficeComponent implements OnInit {
  public offices: ActiveOffice[] = [];
  public isLoading: boolean = false;
  public errorMessage: string | null = null;

  constructor(
    private readonly bookingService: BookingService,
    private readonly router: Router,
  ) {}

  public ngOnInit(): void {
    this.load();
  }

  public selectOffice(office: ActiveOffice): void {
    this.router.navigate(['/book/rooms'], { queryParams: { officeId: office.id, officeName: office.name } });
  }

  private load(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.bookingService.getActiveOffices().subscribe({
      next: (data: ActiveOffice[]) => {
        this.offices = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load offices. Please try again.';
        this.isLoading = false;
      },
    });
  }
}
