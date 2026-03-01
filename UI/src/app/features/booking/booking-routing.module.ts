import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SelectOfficeComponent } from './select-office/select-office.component';
import { SelectRoomComponent } from './select-room/select-room.component';
import { CreateBookingComponent } from './create-booking/create-booking.component';

const routes: Routes = [
  { path: '', component: SelectOfficeComponent },
  { path: 'rooms', component: SelectRoomComponent },
  { path: 'create', component: CreateBookingComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class BookingRoutingModule {}
