import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';

import {
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatRadioModule,
    MatSidenavModule,
    MatMenuModule,
    MatToolbarModule,
    MatTooltipModule,
    MatAutocompleteModule,
    MatSnackBarModule,
    MatListModule,
    MatProgressSpinnerModule
} from '@angular/material';

const MATERIAL_MODULES = [
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatRadioModule,
    MatSidenavModule,
    MatMenuModule,
    MatToolbarModule,
    MatTooltipModule,
    MatAutocompleteModule,
    MatSnackBarModule,
    MatListModule,
    MatProgressSpinnerModule
];

import { TbIconsModule } from '@taskbuilder/icons';

import { TbKendoModule } from './kendo/kendo.module';
export * from './kendo/kendo.module';

/**
 * Components
 */

import { ProxyRouteComponent } from './components/proxy-route/proxy-route.component';
import { DynamicCmpComponent } from './components/dynamic-cmp.component';
import { DynamicCmpComponentTree } from './components/dynamic-cmp.component';
import { ContextMenuComponent } from './components/context-menu/context-menu.component';
import { DocumentComponent } from './components/document.component';
import { PageNotFoundComponent } from './components/page-not-found.component';
import { HeaderStripComponent } from './components/header-strip/header-strip.component';
import { ToolbarTopComponent } from './components/toolbar/toolbar-top/toolbar-top.component';
import { ToolbarTopButtonComponent } from './components/toolbar/toolbar-top/toolbar-top-button/toolbar-top-button.component';
import { ToolbarSeparatorComponent } from './components/toolbar/toolbar-top/toolbar-separator.component';
import { ToolbarBottomComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom.component';
import { ToolbarBottomButtonComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom-button/toolbar-bottom-button.component';
import { TopbarComponent } from './components/topbar/topbar.component';
import { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
import { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
import { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
import { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
import { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
import { BPMIconComponent } from './components/topbar/bpm-icon/bpm-icon.component';
import { UnsupportedComponent } from './components/unsupported.component';
import { UnsupportedFactoryComponent } from './components/unsupported.component';
import { OpenComponent } from './components/explorer/open/open.component';
import { SaveComponent } from './components/explorer/save/save.component';
import { RadarComponent } from './components/radar/radar.component';
import { CultureSelectorComponent } from './components/culture-selector/culture-selector.component';
import { EasyStudioContextComponent } from './components/easystudio-context/easystudio-context.component';
import { CloneDocumentDialogComponent } from './components/clone-document-dialog/clone-document-dialog.component';
import { TbIconComponent } from './components/tb-icon/tb-icon.component';
import { LoadingComponent } from './components/loading/loading.component';
import { DiagnosticComponent } from './components/diagnostic/diagnostic.component';
import { TbHotlinkButtonsComponent } from './controls/hot-link-buttons/tb-hot-link-buttons.component';

// import { TbComponent } from './components/tb.component';
export { TbComponent } from './components/tb.component';

export { BOComponent } from './components/bo/bo.component';
export { BOSlaveComponent } from './components/bo/bo-slave.component';
export { BOCommonComponent } from './components/bo/bo-common.component';
import { BOComponent } from './components/bo/bo.component';
import { BOSlaveComponent } from './components/bo/bo-slave.component';
import { BOCommonComponent } from './components/bo/bo-common.component';

export { ProxyRouteComponent } from './components/proxy-route/proxy-route.component';
export { DynamicCmpComponent } from './components/dynamic-cmp.component';
export { DynamicCmpComponentTree } from './components/dynamic-cmp.component';
export { ContextMenuComponent } from './components/context-menu/context-menu.component';
export { DocumentComponent } from './components/document.component';
export { PageNotFoundComponent } from './components/page-not-found.component';
export { HeaderStripComponent } from './components/header-strip/header-strip.component';
export { ToolbarTopComponent } from './components/toolbar/toolbar-top/toolbar-top.component';
export { ToolbarTopButtonComponent } from './components/toolbar/toolbar-top/toolbar-top-button/toolbar-top-button.component';
export { ToolbarSeparatorComponent } from './components/toolbar/toolbar-top/toolbar-separator.component';
export { ToolbarBottomComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom.component';
export { ToolbarBottomButtonComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom-button/toolbar-bottom-button.component';
export { TopbarComponent } from './components/topbar/topbar.component';
export { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
export { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
export { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
export { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
export { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-element/topbar-menu-elements.component';
export { BPMIconComponent } from './components/topbar/bpm-icon/bpm-icon.component';
export { UnsupportedComponent } from './components/unsupported.component';
export { UnsupportedFactoryComponent } from './components/unsupported.component';
export { OpenComponent } from './components/explorer/open/open.component';
export { SaveComponent } from './components/explorer/save/save.component';
export { RadarComponent } from './components/radar/radar.component';
export { CultureSelectorComponent } from './components/culture-selector/culture-selector.component';
export { EasyStudioContextComponent } from './components/easystudio-context/easystudio-context.component';
export { CloneDocumentDialogComponent } from './components/clone-document-dialog/clone-document-dialog.component';
export { TbIconComponent } from './components/tb-icon/tb-icon.component';
export { LoadingComponent } from './components/loading/loading.component';
export { DiagnosticComponent } from './components/diagnostic/diagnostic.component';
export { TbHotlinkButtonsComponent } from './controls/hot-link-buttons/tb-hot-link-buttons.component';


const TB_COMPONENTS = [
    ProxyRouteComponent, DynamicCmpComponent, DynamicCmpComponentTree, ContextMenuComponent, DocumentComponent, PageNotFoundComponent, HeaderStripComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent, BPMIconComponent,
    UnsupportedComponent, UnsupportedFactoryComponent, OpenComponent, SaveComponent, RadarComponent, CultureSelectorComponent, TbIconComponent,
    BOComponent, BOSlaveComponent, BOCommonComponent, CloneDocumentDialogComponent, EasyStudioContextComponent, LoadingComponent, DiagnosticComponent,
    TbHotlinkButtonsComponent
];

/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import { TbCardContentComponent } from './containers/tb-card/tb-card-content/tb-card-content.component';
import { TbCardFooterComponent } from './containers/tb-card/tb-card-footer/tb-card-footer.component';
import { TbCardHeaderComponent } from './containers/tb-card/tb-card-header/tb-card-header.component';
import { TbCardSubtitleComponent } from './containers/tb-card/tb-card-subtitle/tb-card-subtitle.component';
import { TbCardTitleComponent } from './containers/tb-card/tb-card-title/tb-card-title.component';
import { TbCardComponent } from './containers/tb-card/tb-card.component';
import { TabComponent } from './containers/tabs/tab/tab.component';
import { TabberComponent } from './containers/tabs/tabber/tabber.component';
import { DynamicDialogComponent } from './containers/dynamic-dialog/dynamic-dialog.component';
import { DiagnosticDialogComponent, DiagnosticItemComponent } from './containers/diagnostic-dialog/diagnostic-dialog.component';
import { MessageDialogComponent } from './containers/message-dialog/message-dialog.component';
import { LayoutContainerComponent } from './containers/tiles/layout-container/layout-container.component';
import { TilePanelComponent } from './containers/tiles/tile-panel/tile-panel.component';
import { TileComponent } from './containers/tiles/tile/tile.component';
import { TileGroupComponent } from './containers/tiles/tile-group/tile-group.component';
import { TileManagerComponent } from './containers/tiles/tile-manager/tile-manager.component';
import { DockpaneContainerComponent } from './containers/dockpane/dockpane-container/dockpane-container.component';
import { DockpaneComponent } from './containers/dockpane/dockpane.component';
import { ViewContainerComponent } from './containers/view/view-container/view-container.component';
import { ViewComponent } from './containers/view/view.component';
import { FrameContentComponent } from './containers/frame/frame-content/frame-content.component';
import { FrameComponent } from './containers/frame/frame.component';
import { StatusTilePanelComponent } from './containers/tiles/status-tile-panel/status-tile-panel.component';
import { StatusTileComponent } from './containers/tiles/status-tile/status-tile.component';

export { TbCardContentComponent } from './containers/tb-card/tb-card-content/tb-card-content.component';
export { TbCardFooterComponent } from './containers/tb-card/tb-card-footer/tb-card-footer.component';
export { TbCardHeaderComponent } from './containers/tb-card/tb-card-header/tb-card-header.component';
export { TbCardSubtitleComponent } from './containers/tb-card/tb-card-subtitle/tb-card-subtitle.component';
export { TbCardTitleComponent } from './containers/tb-card/tb-card-title/tb-card-title.component';
export { TbCardComponent } from './containers/tb-card/tb-card.component';
export { TabComponent } from './containers/tabs/tab/tab.component';
export { TabberComponent } from './containers/tabs/tabber/tabber.component';
export { DynamicDialogComponent } from './containers/dynamic-dialog/dynamic-dialog.component';
export { DiagnosticDialogComponent, DiagnosticItemComponent } from './containers/diagnostic-dialog/diagnostic-dialog.component';
export { MessageDialogComponent } from './containers/message-dialog/message-dialog.component';
export { LayoutContainerComponent } from './containers/tiles/layout-container/layout-container.component';
export { TilePanelComponent } from './containers/tiles/tile-panel/tile-panel.component';
export { TileComponent } from './containers/tiles/tile/tile.component';
export { TileGroupComponent } from './containers/tiles/tile-group/tile-group.component';
export { TileManagerComponent } from './containers/tiles/tile-manager/tile-manager.component';
export { DockpaneContainerComponent } from './containers/dockpane/dockpane-container/dockpane-container.component';
export { DockpaneComponent } from './containers/dockpane/dockpane.component';
export { ViewContainerComponent } from './containers/view/view-container/view-container.component';
export { ViewComponent } from './containers/view/view.component';
export { FrameContentComponent } from './containers/frame/frame-content/frame-content.component';
export { FrameComponent } from './containers/frame/frame.component';
export { StatusTilePanelComponent } from './containers/tiles/status-tile-panel/status-tile-panel.component';

const TB_CONTAINERS = [
    FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent, DockpaneComponent, DockpaneContainerComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent,
    MessageDialogComponent, DiagnosticDialogComponent, DiagnosticItemComponent, DynamicDialogComponent, TabberComponent, TabComponent,
    TbCardComponent, TbCardTitleComponent, TbCardSubtitleComponent, TbCardHeaderComponent, TbCardFooterComponent, TbCardContentComponent,
    StatusTilePanelComponent, StatusTileComponent
];

/**
 * TB_CONTROLS
 */
import { LinkComponent } from './controls/link/link.component';
import { LinearGaugeComponent } from './controls/charts/linear-gauge/linear-gauge.component';
import { HotlinkComponent } from './controls/hotlink/hotlink.component';
import { BodyEditComponent } from './controls/body-edit/body-edit.component';
import { BodyEditColumnComponent } from './controls/body-edit-column/body-edit-column.component';
import { TreeViewComponent } from './controls/treeview/tree-view.component';
import { ApplicationDateComponent } from './controls/application-date/application-date.component';
import { StateButtonComponent } from './controls/state-button/state-button.component';
import { ComboSimpleComponent } from './controls/combo-simple/combo-simple.component';
import { SectionTitleComponent } from './controls/section-title/section-title.component';
import { UnknownComponent } from './controls/unknown/unknown.component';
import { MaskedTextBoxComponent } from './controls/masked-text-box/masked-text-box.component';
import { CaptionComponent } from './controls/caption/caption.component';
import { ButtonComponent } from './controls/button/button.component';
import { BoolEditComponent } from './controls/bool-edit/bool-edit.component';
import { FileComponent } from './controls/file/file.component';
import { PlaceholderComponent } from './controls/placeholder/placeholder.component';
import { ImageComponent } from './controls/image/image.component';
import { EnumComboComponent } from './controls/enum-combo/enum-combo.component';
import { ComboComponent } from './controls/combo/combo.component';
import { EmailComponent } from './controls/email/email.component';
import { ColorPickerComponent } from './controls/color-picker/color-picker.component';
import { RadioComponent } from './controls/radio/radio.component';
import { CheckBoxComponent } from './controls/checkbox/checkbox.component';
import { DateInputComponent } from './controls/date-input/date-input.component';
import { TimeInputComponent } from './controls/time-input/time-input.component';
import { TextareaComponent } from './controls/textarea/textarea.component';
import { PasswordComponent } from './controls/password/password.component';
import { PhoneComponent } from './controls/phone/phone.component';
import { NumericTextBoxComponent } from './controls/numeric-text-box/numeric-text-box.component';
import { LabelStaticComponent } from './controls/label-static/label-static.component';
import { TextComponent } from './controls/text/text.component';
import { ConnectionStatusComponent } from './controls/connection-status/connection-status.component';
import { ControlComponent } from './controls/control.component';
import { VATCodeComponent } from './controls/vat-code/vat-code.component';
import { AddressEditComponent } from './controls/address-edit/address-edit.component';
import { ColoredStaticComponent} from './controls/colored-static/coloredstatic.component';
import { PropertyGridComponent } from './controls/property-grid/property-grid.component';
import { PropertyGridItemComponent } from './controls/property-grid/property-grid-item/property-grid-item.component';
import { PropertyGridItemGroupComponent } from './controls/property-grid/property-grid-item-group/property-grid-item-group.component';

export { LinkComponent } from './controls/link/link.component';
export { LinearGaugeComponent } from './controls/charts/linear-gauge/linear-gauge.component';
export { HotlinkComponent } from './controls/hotlink/hotlink.component';
export { BodyEditComponent } from './controls/body-edit/body-edit.component';
export { BodyEditColumnComponent } from './controls/body-edit-column/body-edit-column.component';
export { TreeViewComponent } from './controls/treeview/tree-view.component';
export { ApplicationDateComponent } from './controls/application-date/application-date.component';
export { StateButtonComponent } from './controls/state-button/state-button.component';
export { ComboSimpleComponent } from './controls/combo-simple/combo-simple.component';
export { SectionTitleComponent } from './controls/section-title/section-title.component';
export { UnknownComponent } from './controls/unknown/unknown.component';
export { MaskedTextBoxComponent } from './controls/masked-text-box/masked-text-box.component';
export { CaptionComponent } from './controls/caption/caption.component';
export { ButtonComponent } from './controls/button/button.component';
export { BoolEditComponent } from './controls/bool-edit/bool-edit.component';
export { FileComponent } from './controls/file/file.component';
export { PlaceholderComponent } from './controls/placeholder/placeholder.component';
export { ImageComponent } from './controls/image/image.component';
export { EnumComboComponent } from './controls/enum-combo/enum-combo.component';
export { ComboComponent } from './controls/combo/combo.component';
export { EmailComponent } from './controls/email/email.component';
export { ColorPickerComponent } from './controls/color-picker/color-picker.component';
export { RadioComponent } from './controls/radio/radio.component';
export { CheckBoxComponent } from './controls/checkbox/checkbox.component';
export { DateInputComponent } from './controls/date-input/date-input.component';
export { TimeInputComponent } from './controls/time-input/time-input.component';
export { TextareaComponent } from './controls/textarea/textarea.component';
export { PasswordComponent } from './controls/password/password.component';
export { PhoneComponent } from './controls/phone/phone.component';
export { NumericTextBoxComponent } from './controls/numeric-text-box/numeric-text-box.component';
export { LabelStaticComponent } from './controls/label-static/label-static.component';
export { TextComponent } from './controls/text/text.component';
export { ConnectionStatusComponent } from './controls/connection-status/connection-status.component';
export { ControlComponent } from './controls/control.component';
export { VATCodeComponent } from './controls/vat-code/vat-code.component';
export { AddressEditComponent } from './controls/address-edit/address-edit.component';
export { ColoredStaticComponent} from './controls/colored-static/coloredstatic.component';
export { PropertyGridComponent } from './controls/property-grid/property-grid.component';
export { PropertyGridItemComponent } from './controls/property-grid/property-grid-item/property-grid-item.component';
export { PropertyGridItemGroupComponent } from './controls/property-grid/property-grid-item-group/property-grid-item-group.component';

const TB_CONTROLS = [
    ControlComponent, VATCodeComponent, ColoredStaticComponent, AddressEditComponent,
    ConnectionStatusComponent, TextComponent, UnknownComponent, LabelStaticComponent, CaptionComponent, NumericTextBoxComponent, MaskedTextBoxComponent,
    PhoneComponent, PasswordComponent, SectionTitleComponent, TextareaComponent, TimeInputComponent, DateInputComponent, CheckBoxComponent,
    RadioComponent, ColorPickerComponent, EmailComponent, ComboSimpleComponent, ComboComponent, ButtonComponent, EnumComboComponent,
    ImageComponent, LinkComponent, PlaceholderComponent, StateButtonComponent, FileComponent, BoolEditComponent,
    LinearGaugeComponent, HotlinkComponent, BodyEditComponent, BodyEditColumnComponent, TreeViewComponent, ApplicationDateComponent, PropertyGridComponent,
    PropertyGridItemComponent, PropertyGridItemGroupComponent
];

/**
 * Direttive per style o funzionalità applicate a componenti base
 */
import { ContextMenuDirective } from './directives/context-menu.directive';
import { LayoutTypeVboxDirective, LayoutTypeHboxDirective, LayoutTypeColumnDirective } from './directives/layout-styles';
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './directives/tile-sizes';
import { TdsNormalDirective, TdsFilterDirective, TdsHeaderDirective, TdsFooterDirective, TdsWizardDirective, TdsParametersDirective, TdsBatchDirective } from './directives/tile-dialog-style.directive';
import { TbHotLinkDirective } from './directives/tb-hot-link.directive';
import { OutSideEventHandlerDirective } from './directives/outSideEventHandler.directive';
import { ColoredControlDirective} from './directives/colored-control.directive';

export { ContextMenuDirective } from './directives/context-menu.directive';
export { LayoutTypeVboxDirective, LayoutTypeHboxDirective, LayoutTypeColumnDirective } from './directives/layout-styles';
export { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './directives/tile-sizes';
export { TdsNormalDirective, TdsFilterDirective, TdsHeaderDirective, TdsFooterDirective, TdsWizardDirective, TdsParametersDirective, TdsBatchDirective } from './directives/tile-dialog-style.directive';
export { TbHotLinkDirective } from './directives/tb-hot-link.directive';
export { OutSideEventHandlerDirective } from './directives/outSideEventHandler.directive';
export { ColoredControlDirective} from './directives/colored-control.directive';

const TB_DIRECTIVES = [
    TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
    TdsNormalDirective, TdsFilterDirective, TdsHeaderDirective, TdsFooterDirective, TdsWizardDirective, TdsParametersDirective, TdsBatchDirective,
    LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective, ContextMenuDirective,
    TbHotLinkDirective, OutSideEventHandlerDirective, ColoredControlDirective
];

import { TbColorPipe } from './pipes/tbColor.pipe';
export { TbColorPipe } from './pipes/tbColor.pipe';
import { TbToUpper } from './pipes/toupper.pipe';
export { TbToUpper } from './pipes/toupper.pipe';
import { TbEnumsPipe } from './pipes/enums.pipe';
export { TbEnumsPipe } from './pipes/enums.pipe';
import { TbAsDropDownDataPipe } from './pipes/as-drop-down-data.pipe';
export { TbAsDropDownDataPipe } from './pipes/as-drop-down-data.pipe';

const TB_PIPES = [TbEnumsPipe, TbAsDropDownDataPipe, TbColorPipe, TbToUpper];

export { ComponentInfo } from './models/component-info.model';
export { ContextMenuItem } from './models/context-menu-item.model';
export { ControlTypes } from './models/control-types.enum';
export { CommandEventArgs } from './models/eventargs.model';
export { LoginCompact } from './models/login-compact.model';
export { LoginSession } from './models/login-session.model';
export { MessageDlgArgs, MessageDlgResult, DiagnosticData, Message, DiagnosticDlgResult, DiagnosticType } from './models/message-dialog.model';
export { OperationResult } from './models/operation-result.model';
export { StateButton } from './models/state-button.model';
export { ViewModeType } from './models/view-mode-type.model';
export { SocketConnectionStatus } from './models/websocket-connection.enum';
export { FormMode } from './models/form-mode.enum';
export * from './models/store.models';
export * from './commons/selector';

/**
 * NON ESPORTARE
 */
const _MODULES = [
    CommonModule,
    FormsModule,
    HttpModule,
    RouterModule,
    TbIconsModule
];

import { TreeModule } from 'angular-tree-component';

@NgModule({
    imports: [_MODULES, MATERIAL_MODULES, TbKendoModule, TreeModule],
    declarations: [TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES, CloneDocumentDialogComponent, TB_PIPES],
    exports: [MATERIAL_MODULES, TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS, TB_DIRECTIVES, TbKendoModule],
    entryComponents: [UnsupportedComponent, RadarComponent, CloneDocumentDialogComponent, TbHotlinkButtonsComponent]
})
export class TbSharedModule { }
