import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';

import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent } from './containers';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';

import { TopbarComponent, TopbarMenuComponent, TopbarMenuUserComponent, TopbarMenuAppComponent } from './topbar/index';
// import { TopbarButtonComponent } from './topbar/topbar-button.component';

import { SidenavService } from '../core/sidenav.service';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [
    PageNotFoundComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuUserComponent, TopbarMenuAppComponent,
    TabComponent, TabberComponent, DynamicCmpComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent
  ],
  exports: [
    CommonModule, PageNotFoundComponent, TabComponent, TabberComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuUserComponent, TopbarMenuAppComponent,
    DynamicCmpComponent
  ]
})
export class SharedModule {

  constructor( @Optional() @SkipSelf() parentModule: SharedModule) {
    if (parentModule) {
      throw new Error(
        'SharedModule is already loaded. Import it in the AppModule only');
    }
  }

}
