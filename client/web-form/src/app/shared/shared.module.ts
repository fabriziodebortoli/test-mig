import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { TopbarSearchComponent } from './topbar/topbar-search/topbar-search.component';
import { ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent } from './toolbar';
import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent, ViewComponent, TilePanelComponent, FrameComponent } from './containers';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { OpenComponent, SaveComponent } from './explorer';
import { TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent } from './topbar/index';
import { HeaderStripComponent } from './header-strip/header-strip.component';

const TB_COMPONENTS = [
  PageNotFoundComponent,
  TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarSearchComponent,
  ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
  TabComponent, TabberComponent, ViewComponent, FrameComponent,
  DynamicCmpComponent,
  EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
  TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent,
  HeaderStripComponent,
  OpenComponent, SaveComponent
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot(),
    RouterModule
  ],
  declarations: [TB_COMPONENTS],
  exports: [TB_COMPONENTS]
})
export class SharedModule {

  constructor() {

  }

}
