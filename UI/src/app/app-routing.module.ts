import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    redirectTo: '/my-bookings',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'my-bookings',
    loadChildren: () => import('./features/my-booking/my-booking.module').then(m => m.MyBookingModule),
    canActivate: [authGuard]
  },
  {
    path: 'users',
    loadChildren: () => import('./features/users/users.module').then(m => m.UsersModule),
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'offices',
    loadChildren: () => import('./features/offices/offices.module').then(m => m.OfficesModule),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'OfficeManager'] }
  },
  {
    path: 'book',
    loadChildren: () => import('./features/booking/booking.module').then(m => m.BookingModule),
    canActivate: [authGuard]
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./shared/components/unauthorized/unauthorized.component').then(
        m => m.UnauthorizedComponent
      )
  },
  {
    path: '**',
    redirectTo: '/my-bookings'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
