import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';

import { MyBookingRoutingModule } from './my-booking-routing.module';
import { MyBookingComponent } from './my-booking.component';

@NgModule({
  declarations: [MyBookingComponent],
  imports: [
    CommonModule,
    RouterModule,
    MyBookingRoutingModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule
  ]
})
export class MyBookingModule {}
