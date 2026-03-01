import { Component, OnInit, AfterViewInit, OnDestroy, ViewChild } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort, Sort } from '@angular/material/sort';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { CreateUserDialogComponent } from '../create-user-dialog/create-user-dialog.component';
import { EditUserDialogComponent } from '../edit-user-dialog/edit-user-dialog.component';
import { UsersService } from '../../../core/services/users.service';
import { User, PagedResult } from '../../../models/entities.model';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit, AfterViewInit, OnDestroy {
  public readonly displayedColumns: string[] = ['fullName', 'email', 'role', 'status', 'createdAt', 'actions'];
  public readonly dataSource: MatTableDataSource<User> = new MatTableDataSource<User>();
  public isLoading: boolean = true;
  public loadError: string | null = null;
  public totalCount: number = 0;

  private currentPage: number = 1;
  private currentPageSize: number = 10;
  private currentSearch: string = '';
  private currentSortBy: string = 'lastName';
  private currentSortDir: string = 'asc';

  private readonly searchSubject: Subject<string> = new Subject<string>();
  private readonly destroy$: Subject<void> = new Subject<void>();

  @ViewChild(MatSort) private sort!: MatSort;
  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  public constructor(
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
    private readonly usersService: UsersService
  ) {}

  public ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe((search: string): void => {
      this.currentSearch = search;
      this.currentPage = 1;
      this.loadUsers();
    });

    this.loadUsers();
  }

  public ngAfterViewInit(): void {
    this.paginator.page
      .pipe(takeUntil(this.destroy$))
      .subscribe((e: PageEvent): void => {
        this.currentPage = e.pageIndex + 1;
        this.currentPageSize = e.pageSize;
        this.loadUsers();
      });

    this.sort.sortChange
      .pipe(takeUntil(this.destroy$))
      .subscribe((s: Sort): void => {
        this.currentSortBy = this.mapSortColumn(s.active);
        this.currentSortDir = s.direction || 'asc';
        this.currentPage = 1;
        this.paginator.firstPage();
        this.loadUsers();
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
      fullName: 'lastName',
      email: 'email',
      role: 'role',
      status: 'status',
      createdAt: 'createdAt',
    };
    return map[column] ?? 'lastName';
  }

  private loadUsers(): void {
    this.isLoading = true;
    this.loadError = null;
    this.usersService.getUsers(
      this.currentPage,
      this.currentPageSize,
      this.currentSearch,
      this.currentSortBy,
      this.currentSortDir
    ).subscribe({
      next: (result: PagedResult<User>): void => {
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: (): void => {
        this.loadError = 'Failed to load users. Please refresh the page.';
        this.isLoading = false;
      }
    });
  }

  public openCreateUserDialog(): void {
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result: User | undefined): void => {
      if (result) {
        this.loadUsers();
        this.snackBar.open(
          `User ${result.firstName} ${result.lastName} created successfully.`,
          'Dismiss',
          { duration: 5000, panelClass: ['success-snackbar'] }
        );
      }
    });
  }

  public openEditUserDialog(user: User): void {
    const dialogRef = this.dialog.open(EditUserDialogComponent, {
      disableClose: true,
      data: user
    });

    dialogRef.afterClosed().subscribe((result: User | undefined): void => {
      if (result) {
        this.loadUsers();
        this.snackBar.open(
          `User ${result.firstName} ${result.lastName} updated successfully.`,
          'Dismiss',
          { duration: 5000, panelClass: ['success-snackbar'] }
        );
      }
    });
  }
}
