import { Component, OnInit, AfterViewInit, OnDestroy, ViewChild } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort, Sort } from '@angular/material/sort';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { OfficesService } from '../../../core/services/offices.service';
import { AuthService } from '../../../core/services/auth.service';
import { Office, PagedResult } from '../../../models/entities.model';
import { OfficeDialogComponent, OfficeDialogData } from '../office-dialog/office-dialog.component';

@Component({
  selector: 'app-offices-page',
  templateUrl: './offices-page.component.html',
  styleUrls: ['./offices-page.component.scss'],
})
export class OfficesPageComponent implements OnInit, AfterViewInit, OnDestroy {
  public readonly displayedColumns: string[] = ['name', 'address', 'roomCount', 'managers', 'status', 'actions'];
  public readonly dataSource: MatTableDataSource<Office> = new MatTableDataSource<Office>();
  public isLoading: boolean = true;
  public loadError: string | null = null;
  public totalCount: number = 0;
  public readonly isAdmin: boolean;

  private currentPage: number = 1;
  private currentPageSize: number = 10;
  private currentSearch: string = '';
  private currentSortBy: string = 'name';
  private currentSortDir: string = 'asc';

  private readonly searchSubject: Subject<string> = new Subject<string>();
  private readonly destroy$: Subject<void> = new Subject<void>();

  @ViewChild(MatSort) private sort!: MatSort;
  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  public constructor(
    private readonly officesService: OfficesService,
    private readonly authService: AuthService,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
  ) {
    this.isAdmin = authService.getCurrentUser()?.role === 'Admin';
  }

  public ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe((search: string): void => {
      this.currentSearch = search;
      this.currentPage = 1;
      this.loadOffices();
    });

    this.loadOffices();
  }

  public ngAfterViewInit(): void {
    this.paginator.page
      .pipe(takeUntil(this.destroy$))
      .subscribe((e: PageEvent): void => {
        this.currentPage = e.pageIndex + 1;
        this.currentPageSize = e.pageSize;
        this.loadOffices();
      });

    this.sort.sortChange
      .pipe(takeUntil(this.destroy$))
      .subscribe((s: Sort): void => {
        this.currentSortBy = this.mapSortColumn(s.active);
        this.currentSortDir = s.direction || 'asc';
        this.currentPage = 1;
        this.paginator.firstPage();
        this.loadOffices();
      });
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public onSearchInput(event: Event): void {
    this.searchSubject.next((event.target as HTMLInputElement).value);
  }

  private mapSortColumn(column: string): string {
    const map: Record<string, string> = {
      name: 'name',
      address: 'address',
      roomCount: 'roomCount',
      status: 'status',
    };
    return map[column] ?? 'name';
  }

  private loadOffices(): void {
    this.isLoading = true;
    this.loadError = null;
    this.officesService.getOffices(
      this.currentPage,
      this.currentPageSize,
      this.currentSearch,
      this.currentSortBy,
      this.currentSortDir
    ).subscribe({
      next: (result: PagedResult<Office>): void => {
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: (): void => {
        this.loadError = 'Failed to load offices. Please refresh the page.';
        this.isLoading = false;
      },
    });
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
        this.loadOffices();
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
