import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';

import { BookingRoutingModule } from './booking-routing.module';
import { SelectOfficeComponent } from './select-office/select-office.component';
import { SelectRoomComponent } from './select-room/select-room.component';
import { CreateBookingComponent } from './create-booking/create-booking.component';

@NgModule({
  declarations: [SelectOfficeComponent, SelectRoomComponent, CreateBookingComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    BookingRoutingModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatNativeDateModule,
    MatProgressBarModule,
    MatSelectModule,
    MatTooltipModule,
  ],
})
export class BookingModule {}

