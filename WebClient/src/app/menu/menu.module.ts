import { FormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { MenuComponent } from './components/menu/menu.component';
import { LoginComponent } from './components/login/login.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
  ],
  declarations: [LoginComponent, MenuComponent],
  exports:[LoginComponent, MenuComponent]
})
export class MenuModule { }
