import { TopbarSearchComponent } from './topbar/topbar-search/topbar-search.component';
import { ToolbarTopComponent, ToolbarButtonComponent, HistoryComponent, ToolbarSeparatorComponent, ToolbarBottomComponent } from './toolbar';
import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';

import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent, ViewComponent, FrameComponent } from './containers';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { TopbarComponent, TopbarMenuComponent, TopbarMenuUserComponent, TopbarMenuAppComponent } from './topbar/index';

import { SidenavService } from '../core/sidenav.service';

const TB_COMPONENTS = [
  PageNotFoundComponent,
  TopbarComponent, TopbarMenuComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarSearchComponent,
  ToolbarTopComponent, ToolbarButtonComponent, HistoryComponent, ToolbarSeparatorComponent, ToolbarBottomComponent,
  TabComponent, TabberComponent, ViewComponent, FrameComponent,
  DynamicCmpComponent,
  EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
  TileManagerComponent, TileGroupComponent, TileComponent
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [TB_COMPONENTS],
  exports: [TB_COMPONENTS]
})
export class SharedModule {

  constructor() {

  }

}
