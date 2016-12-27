import { ToolbarButtonComponent } from './toolbar/toolbar-button.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';

import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent } from './containers';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { TopbarComponent } from './topbar/topbar.component';

import { SidenavService } from '../core/sidenav.service';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [
    PageNotFoundComponent, TopbarComponent, ToolbarComponent, ToolbarButtonComponent,
    TabComponent, TabberComponent, DynamicCmpComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent
  ],
  exports: [
    CommonModule, PageNotFoundComponent, TabComponent, TabberComponent, ToolbarComponent, ToolbarButtonComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent,
    TopbarComponent, DynamicCmpComponent
  ]
})
export class SharedModule {

  constructor( @Optional() @SkipSelf() parentModule: SharedModule) {
    
  }

}
