import { Component, OnInit, AfterViewInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { CreateUserDialogComponent } from '../create-user-dialog/create-user-dialog.component';
import { EditUserDialogComponent } from '../edit-user-dialog/edit-user-dialog.component';
import { UsersService } from '../../../core/services/users.service';
import { User } from '../../../models/entities.model';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit, AfterViewInit {
  public readonly displayedColumns: string[] = ['fullName', 'email', 'role', 'status', 'createdAt', 'actions'];
  public readonly dataSource: MatTableDataSource<User> = new MatTableDataSource<User>();
  public isLoading: boolean = true;
  public loadError: string | null = null;

  @ViewChild(MatSort) private sort!: MatSort;
  @ViewChild(MatPaginator) private paginator!: MatPaginator;

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private usersService: UsersService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sortingDataAccessor = (user: User, column: string): string | number => {
      switch (column) {
        case 'fullName': return `${user.lastName} ${user.firstName}`;
        case 'createdAt': return user.createdAt?.toString() ?? '';
        default: return (user as any)[column] ?? '';
      }
    };
  }

  loadUsers(): void {
    this.isLoading = true;
    this.loadError = null;
    this.usersService.getUsers().subscribe({
      next: (users: User[]): void => {
        this.dataSource.data = users;
        this.isLoading = false;
      },
      error: (): void => {
        this.loadError = 'Failed to load users. Please refresh the page.';
        this.isLoading = false;
      }
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  public openCreateUserDialog(): void {
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((result: User | undefined): void => {
      if (result) {
        this.dataSource.data = [result, ...this.dataSource.data];
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
        const data = [...this.dataSource.data];
        const index = data.findIndex(u => u.id === result.id);
        if (index !== -1) data[index] = result;
        this.dataSource.data = data;
        this.snackBar.open(
          `User ${result.firstName} ${result.lastName} updated successfully.`,
          'Dismiss',
          { duration: 5000, panelClass: ['success-snackbar'] }
        );
      }
    });
  }
}
