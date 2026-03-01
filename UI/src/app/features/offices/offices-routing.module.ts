import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OfficesPageComponent } from './offices-page/offices-page.component';

const routes: Routes = [
  {
    path: '',
    component: OfficesPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OfficesRoutingModule {}
