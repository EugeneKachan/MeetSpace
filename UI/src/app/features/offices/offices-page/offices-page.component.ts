import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { OfficesService } from '../../../core/services/offices.service';
import { AuthService } from '../../../core/services/auth.service';
import { Office } from '../../../models/entities.model';
import { OfficeDialogComponent, OfficeDialogData } from '../office-dialog/office-dialog.component';

@Component({
  selector: 'app-offices-page',
  templateUrl: './offices-page.component.html',
  styleUrls: ['./offices-page.component.scss'],
})
export class OfficesPageComponent implements OnInit, AfterViewInit {
  public readonly displayedColumns: string[] = ['name', 'address', 'roomCount', 'managers', 'status', 'actions'];
  public readonly dataSource: MatTableDataSource<Office> = new MatTableDataSource<Office>();
  public isLoading: boolean = true;
  public loadError: string | null = null;
  public readonly isAdmin: boolean;

  @ViewChild(MatSort) private sort!: MatSort;
  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  constructor(
    private officesService: OfficesService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
  ) {
    this.isAdmin = authService.getCurrentUser()?.role === 'Admin';
  }

  public ngOnInit(): void {
    this.loadOffices();
  }

  public ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sortingDataAccessor = (office: Office, column: string): string | number => {
      switch (column) {
        case 'roomCount': return office.rooms?.length ?? 0;
        case 'managers': return office.managers?.length ?? 0;
        default: return (office as any)[column] ?? '';
      }
    };
  }

  public loadOffices(): void {
    this.isLoading = true;
    this.loadError = null;
    this.officesService.getOffices().subscribe({
      next: (offices: Office[]): void => {
        this.dataSource.data = offices;
        this.isLoading = false;
      },
      error: (): void => {
        this.loadError = 'Failed to load offices. Please refresh the page.';
        this.isLoading = false;
      },
    });
  }

  public applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  public openCreateOfficeDialog(): void {
    const data: OfficeDialogData = { office: null };
    const dialogRef = this.dialog.open(OfficeDialogComponent, {
      width: '700px',
      disableClose: true,
      data,
    });

    dialogRef.afterClosed().subscribe((created: boolean): void => {
      if (created) {
        this.loadOffices();
        this.snackBar.open('Office created successfully.', 'Dismiss', {
          duration: 5000,
          panelClass: ['success-snackbar'],
        });
      }
    });
  }

  public openEditOfficeDialog(office: Office): void {
    const data: OfficeDialogData = { office };
    const dialogRef = this.dialog.open(OfficeDialogComponent, {
      width: '700px',
      disableClose: true,
      data,
    });

    dialogRef.afterClosed().subscribe((updated: boolean): void => {
      if (updated) {
        this.loadOffices();
        this.snackBar.open('Changes saved.', 'Dismiss', {
          duration: 5000,
          panelClass: ['success-snackbar'],
        });
      }
    });
  }

  public deactivateOffice(office: Office): void {
    if (!confirm(`Deactivate office "${office.name}"? All associated rooms will also be deactivated.`)) {
      return;
    }
    this.officesService.deactivateOffice(office.id).subscribe({
      next: (): void => {
        const data = this.dataSource.data.map((o: Office) =>
          o.id === office.id ? { ...o, isActive: false } : o,
        );
        this.dataSource.data = data;
        this.snackBar.open(`Office "${office.name}" deactivated.`, 'Dismiss', {
          duration: 5000,
          panelClass: ['success-snackbar'],
        });
      },
      error: (): void => {
        this.snackBar.open('Failed to deactivate office. Please try again.', 'Dismiss', {
          duration: 5000,
          panelClass: ['error-snackbar'],
        });
      },
    });
  }
}
