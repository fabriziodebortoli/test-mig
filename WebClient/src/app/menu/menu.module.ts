import { MenuService } from './services/menu.service';
import { SharedModule } from 'tb-shared';
import { FormsModule } from '@angular/forms';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuSelectorComponent } from './components/menu/menu-selector/menu-selector.component';
import { TileContainerComponent } from './components/menu/tile-container/tile-container.component';

import { LoginComponent } from './components/login/login.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    MaterialModule.forRoot()
  ],

  declarations: [LoginComponent, MenuComponent, ApplicationSelectorComponent, GroupSelectorComponent, MenuSelectorComponent, TileContainerComponent],
  exports: [LoginComponent, MenuComponent, ApplicationSelectorComponent, GroupSelectorComponent, MenuSelectorComponent, TileContainerComponent],
  providers:[MenuService]
})
export class MenuModule { }

