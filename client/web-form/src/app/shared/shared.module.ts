
import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';


import { TopbarSearchComponent } from './topbar/topbar-search/topbar-search.component';
import { ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent } from './toolbar';
import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent, ViewComponent, TilePanelComponent, FrameComponent } from './containers';
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './containers/tiles/tile/tile.size';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, StateButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent } from './topbar/index';
import { HeaderStripComponent } from './header-strip/header-strip.component';
import { OpenComponent } from './explorer/open/open.component';
import { SaveComponent } from './explorer/save/save.component';
import { GridComponent } from './controls/grid/grid.component';
import { CaptionComponent } from './controls/caption/caption.component';
import { PlaceholderComponent } from './controls/placeholder/placeholder.component';
import { PasswordComponent } from './controls/password/password.component';


const KENDO_UI_MODULES = [
  GridModule,
  InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule
];

const TB_COMPONENTS = [
  PageNotFoundComponent,
  TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarSearchComponent,
  ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
  TabComponent, TabberComponent, ViewComponent, FrameComponent,
  DynamicCmpComponent,
  EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, GridComponent,
  TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent,
  TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
  HeaderStripComponent,
  OpenComponent, SaveComponent
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot(),
    RouterModule,
    KENDO_UI_MODULES
  ],
  declarations: [TB_COMPONENTS, StateButtonComponent, CaptionComponent, PlaceholderComponent, PasswordComponent],
  exports: [TB_COMPONENTS],
  entryComponents: [OpenComponent, SaveComponent]
})
export class SharedModule {

  constructor() {

  }

}
