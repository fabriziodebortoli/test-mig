import { NameDirective } from './directives/name-directive';

import {
  FrameContentComponent, ViewContainerComponent, DockpaneContainerComponent,
  DockpaneComponent, TileManagerComponent,
  TileGroupComponent, TileComponent, ViewComponent,
  TilePanelComponent, LayoutContainerComponent, FrameComponent
} from './containers';

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { ChartsModule } from '@progress/kendo-angular-charts';

import { ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent } from './toolbar';
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './containers/tiles/tile/tile.size';
import { LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective } from './containers/tiles/layout-styles';
import {
  CaptionComponent, ComboComponent, EnumComboComponent, RadioComponent, ImageComponent,
  CheckBoxComponent, ButtonComponent, StateButtonComponent, TextComponent, ColorPickerComponent, BoolEditComponent, UnknownComponent, BodyEditComponent,
  LinearGaugeComponent, ConnectionStatusComponent
} from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent } from './topbar/index';
import { HeaderStripComponent } from './header-strip/header-strip.component';
import { OpenComponent } from './explorer/open/open.component';
import { SaveComponent } from './explorer/save/save.component';
import { GridComponent } from './controls/grid/grid.component';
import { PlaceholderComponent } from './controls/placeholder/placeholder.component';
import { PasswordComponent } from './controls/password/password.component';
import { MaskedTextBoxComponent } from './controls/masked-text-box/masked-text-box.component';
import { NumericTextBoxComponent } from './controls/numeric-text-box/numeric-text-box.component';
import { DateInputComponent } from './controls/date-input/date-input.component';
import { ContextMenuComponent } from './context-menu/context-menu.component';
import { LabelStaticComponent } from './controls/label-static/label-static.component';

import { ContextMenuDirective } from './directives/context-menu.directive';

import { PhoneComponent } from './controls/phone/phone.component';
import { EmailComponent } from './controls/email/email.component';
import { TimeInputComponent } from './controls/time-input/time-input.component';
import { SectionTitleComponent } from './controls/section-title/section-title.component';
import { TextareaComponent } from './controls/textarea/textarea.component';
import { FileComponent } from './controls/file/file.component';
import { LinkComponent } from './controls/link/link.component';
import { ComboSimpleComponent } from './controls/combo-simple/combo-simple.component';
import { MessageDialogComponent } from './containers/message-dialog/message-dialog.component';

import { TopbarMenuElementsComponent } from './topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
import { TbCardComponent } from './containers/tb-card/tb-card.component';
import { TbCardTitleComponent } from './containers/tb-card/tb-card-title/tb-card-title.component';
import { TbCardContentComponent } from './containers/tb-card/tb-card-content/tb-card-content.component';
import { IconComponent } from './icons/icon.component';
import { M4IconComponent } from './icons/m4-icon.component';
import { HotlinkComponent } from './controls/hotlink/hotlink.component';

const KENDO_UI_MODULES = [
  GridModule,
  InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule,
  PopupModule,
  ButtonsModule,
  ChartsModule
];

const TB_COMPONENTS = [
  PageNotFoundComponent,
  TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent,
  FrameContentComponent, ViewContainerComponent, DockpaneContainerComponent,
  ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
  ViewComponent, DockpaneComponent, FrameComponent, FrameContentComponent, ViewContainerComponent, DockpaneContainerComponent,
  DynamicCmpComponent, UnknownComponent,
  CaptionComponent, ComboComponent, EnumComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, GridComponent, DateInputComponent,
  OpenComponent, SaveComponent, StateButtonComponent, TextComponent, LabelStaticComponent, TimeInputComponent,
  TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent, HeaderStripComponent,
  PlaceholderComponent, PasswordComponent, MaskedTextBoxComponent, NumericTextBoxComponent, ContextMenuComponent, ImageComponent, ColorPickerComponent,
  BoolEditComponent, BodyEditComponent, LinkComponent, LinearGaugeComponent,
  PhoneComponent, EmailComponent, SectionTitleComponent, TextareaComponent, FileComponent, ComboSimpleComponent, MessageDialogComponent,
  IconComponent, TopbarMenuElementsComponent, TbCardComponent, TbCardTitleComponent, TbCardContentComponent,
  M4IconComponent, ConnectionStatusComponent, HotlinkComponent

];

const TB_DIRECTIVES = [
  TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
  LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective,
  ContextMenuDirective, NameDirective
];

@NgModule({
  imports: [
    ReactiveFormsModule, InputsModule,
    DialogModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    KENDO_UI_MODULES
  ],
  declarations: [TB_COMPONENTS, TB_DIRECTIVES, MessageDialogComponent],
  exports: [TB_COMPONENTS, TB_DIRECTIVES],
  entryComponents: [OpenComponent, SaveComponent, ContextMenuComponent]

})
export class SharedModule {

  constructor() {

  }

}
