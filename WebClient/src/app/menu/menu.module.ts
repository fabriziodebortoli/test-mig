import { MenuService } from './services/menu.service';
import { SharedModule } from './../shared/shared.module';
import { FormsModule } from '@angular/forms';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuSelectorComponent } from './components/menu/menu-selector/menu-selector.component';

import { LoginComponent } from './components/login/login.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
  ],

  declarations: [LoginComponent, MenuComponent, ApplicationSelectorComponent, GroupSelectorComponent, MenuSelectorComponent],
  exports: [LoginComponent, MenuComponent, ApplicationSelectorComponent, GroupSelectorComponent, MenuSelectorComponent],
  providers:[MenuService]
})
export class MenuModule { }

