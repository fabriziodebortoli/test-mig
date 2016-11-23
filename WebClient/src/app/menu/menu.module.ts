import { MenuService } from './services/menu.service';
import { SharedModule } from 'tb-shared';
import { FormsModule } from '@angular/forms';
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
  exports: [LoginComponent, MenuComponent],
  providers:[MenuService]
})
export class MenuModule { }
