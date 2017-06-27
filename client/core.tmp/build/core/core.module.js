import { NgModule, Optional, SkipSelf } from '@angular/core';
/**
 * Servizi
 *
 * Tutti i servizi condivisi TB (http, websocket,
 */
import { BOService, BOHelperService, ComponentService, DataService, DocumentService, EnumsService, EventDataService, ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, SidenavService, TabberService, UtilsService, WebSocketService } from './services/index';
export { BOService, BOClient } from './services/bo.service';
export { BOHelperService } from './services/bohelper.service';
export { ComponentService, ComponentCreatedArgs } from './services/component.service';
export { DataService } from './services/data.service';
export { DocumentService } from './services/document.service';
export { EnumsService } from './services/enums.service';
export { EventDataService } from './services/eventdata.service';
export { ExplorerService } from './services/explorer.service';
export { HttpService } from './services/http.service';
export { InfoService } from './services/info.service';
export { LayoutService } from './services/layout.service';
export { Logger } from './services/logger.service';
export { LoginSessionService } from './services/login-session.service';
export { SidenavService } from './services/sidenav.service';
export { TabberService } from './services/tabber.service';
export { UtilsService } from './services/utils.service';
export { WebSocketService, SocketMessage } from './services/websocket.service';
const /** @type {?} */ TB_SERVICES = [
    BOService, BOHelperService, ComponentService, DataService, DocumentService, EnumsService, EventDataService,
    ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, SidenavService,
    TabberService, UtilsService, WebSocketService
];
/**
 * Guards
 */
import { CoreGuard } from './guards/index';
export { CoreGuard } from './guards/core.guard';
const /** @type {?} */ TB_GUARDS = [
    CoreGuard
];
/**
 * Containers - Contenitori di struttura della pagina derivati dalla versione desktop
 */
import { FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent, DockpaneComponent, DockpaneContainerComponent, TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent, MessageDialogComponent } from './containers/index';
export { FrameComponent } from './containers/frame/frame.component';
export { FrameContentComponent } from './containers/frame/frame-content/frame-content.component';
export { ViewComponent } from './containers/view/view.component';
export { ViewContainerComponent } from './containers/view/view-container/view-container.component';
export { DockpaneComponent } from './containers/dockpane/dockpane.component';
export { DockpaneContainerComponent } from './containers/dockpane/dockpane-container/dockpane-container.component';
export { TileManagerComponent } from './containers/tiles/tile-manager/tile-manager.component';
export { TileGroupComponent } from './containers/tiles/tile-group/tile-group.component';
export { TileComponent } from './containers/tiles/tile/tile.component';
export { TilePanelComponent } from './containers/tiles/tile-panel/tile-panel.component';
export { LayoutContainerComponent } from './containers/tiles/layout-container/layout-container.component';
export { MessageDialogComponent, MessageDlgArgs, MessageDlgResult } from './containers/message-dialog/message-dialog.component';
const /** @type {?} */ TB_CONTAINERS = [
    DockpaneComponent, DockpaneContainerComponent, FrameComponent, FrameContentComponent, MessageDialogComponent,
    TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, ViewComponent, ViewContainerComponent,
    LayoutContainerComponent
];
/**
 * Componenti generici
 */
import { TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent, DynamicCmpComponent, HeaderStripComponent, AccordionComponent, AccordionGroupComponent, ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent, OpenComponent, SaveComponent, ContextMenuComponent, PageNotFoundComponent } from './components/index';
export { BOComponent } from './components/bo.component';
export { DocumentComponent } from './components/document.component';
export { DynamicCmpComponent } from './components/dynamic-cmp.component';
export { PageNotFoundComponent } from './components/page-not-found.component';
export { TbComponent } from './components/tb.component';
export { TopbarComponent } from './components/topbar/topbar.component';
export { TopbarMenuComponent } from './components/topbar/topbar-menu/topbar-menu.component';
export { TopbarMenuAppComponent } from './components/topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
export { TopbarMenuElementsComponent } from './components/topbar/topbar-menu/topbar-menu-elements/topbar-menu-elements.component';
export { TopbarMenuTestComponent } from './components/topbar/topbar-menu/topbar-menu-test/topbar-menu-test.component';
export { TopbarMenuUserComponent } from './components/topbar/topbar-menu/topbar-menu-user/topbar-menu-user.component';
export { ContextMenuComponent } from './components/context-menu/context-menu.component';
export { SaveComponent } from './components/explorer/save/save.component';
export { OpenComponent } from './components/explorer/open/open.component';
export { AccordionComponent, AccordionGroupComponent } from './components/accordion/accordion.component';
export { HeaderStripComponent } from './components/header-strip/header-strip.component';
export { ToolbarTopComponent } from './components/toolbar/toolbar-top/toolbar-top.component';
export { ToolbarSeparatorComponent } from './components/toolbar/toolbar-top/toolbar-separator.component';
export { ToolbarTopButtonComponent } from './components/toolbar/toolbar-top/toolbar-top-button/toolbar-top-button.component';
export { ToolbarBottomComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom.component';
export { ToolbarBottomButtonComponent } from './components/toolbar/toolbar-bottom/toolbar-bottom-button/toolbar-bottom-button.component';
const /** @type {?} */ TB_COMPONENTS = [
    TopbarComponent, TopbarMenuComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, TopbarMenuAppComponent,
    TopbarMenuElementsComponent, DynamicCmpComponent, HeaderStripComponent, AccordionComponent, AccordionGroupComponent,
    ToolbarTopComponent, ToolbarTopButtonComponent, ToolbarSeparatorComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent,
    OpenComponent, SaveComponent, ContextMenuComponent, PageNotFoundComponent, PageNotFoundComponent
];
/**
 * Controls derivati dai parsedControl componenti per Form
 */
import { CaptionComponent, ComboComponent, EnumComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, StateButtonComponent, TextComponent, BoolEditComponent, BodyEditComponent, LinearGaugeComponent, ImageComponent, ColorPickerComponent, GridComponent, DateInputComponent, LabelStaticComponent, TimeInputComponent, PlaceholderComponent, PasswordComponent, MaskedTextBoxComponent, NumericTextBoxComponent, LinkComponent, PhoneComponent, EmailComponent, SectionTitleComponent, TextareaComponent, FileComponent, ComboSimpleComponent, TbCardComponent, TbCardTitleComponent, TbCardContentComponent, UnknownComponent } from './controls/index';
export { BodyEditComponent } from './controls/body-edit/body-edit.component';
export { BoolEditComponent } from './controls/bool-edit/bool-edit.component';
export { ButtonComponent } from './controls/button/button.component';
export { CaptionComponent } from './controls/caption/caption.component';
export { LinearGaugeComponent } from './controls/charts/linear-gauge/linear-gauge.component';
export { CheckBoxComponent } from './controls/checkbox/checkbox.component';
export { ColorPickerComponent } from './controls/color-picker/color-picker.component';
export { ComboComponent } from './controls/combo/combo.component';
export { ComboSimpleComponent } from './controls/combo-simple/combo-simple.component';
export { DateInputComponent } from './controls/date-input/date-input.component';
export { EmailComponent } from './controls/email/email.component';
export { EnumComboComponent } from './controls/enum-combo/enum-combo.component';
export { FileComponent } from './controls/file/file.component';
export { GridComponent } from './controls/grid/grid.component';
export { ImageComponent } from './controls/image/image.component';
export { LabelStaticComponent } from './controls/label-static/label-static.component';
export { LinkComponent } from './controls/link/link.component';
export { MaskedTextBoxComponent } from './controls/masked-text-box/masked-text-box.component';
export { NumericTextBoxComponent } from './controls/numeric-text-box/numeric-text-box.component';
export { PasswordComponent } from './controls/password/password.component';
export { PhoneComponent } from './controls/phone/phone.component';
export { PlaceholderComponent } from './controls/placeholder/placeholder.component';
export { RadioComponent } from './controls/radio/radio.component';
export { SectionTitleComponent } from './controls/section-title/section-title.component';
export { StateButtonComponent } from './controls/state-button/state-button.component';
export { TbCardComponent } from './controls/tb-card/tb-card.component';
export { TbCardContentComponent } from './controls/tb-card/tb-card-content/tb-card-content.component';
export { TbCardTitleComponent } from './controls/tb-card/tb-card-title/tb-card-title.component';
export { TextComponent } from './controls/text/text.component';
export { TextareaComponent } from './controls/textarea/textarea.component';
export { TimeInputComponent } from './controls/time-input/time-input.component';
export { UnknownComponent } from './controls/unknown/unknown.component';
const /** @type {?} */ TB_CONTROLS = [
    CaptionComponent, ComboComponent, EnumComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    StateButtonComponent, TextComponent, BoolEditComponent, BodyEditComponent, LinearGaugeComponent,
    ImageComponent, ColorPickerComponent, GridComponent, DateInputComponent, LabelStaticComponent, TimeInputComponent,
    PlaceholderComponent, PasswordComponent, MaskedTextBoxComponent, NumericTextBoxComponent,
    LinkComponent, PhoneComponent, EmailComponent, SectionTitleComponent, TextareaComponent, FileComponent, ComboSimpleComponent,
    TbCardComponent, TbCardTitleComponent, TbCardContentComponent, UnknownComponent
];
export class TbCoreModule {
    /**
     * @param {?} parentModule
     */
    constructor(parentModule) {
        if (parentModule) {
            throw new Error('TbCoreModule is already loaded. Import it in the AppModule only');
        }
    }
    /**
     * @return {?}
     */
    static forRoot() {
        return {
            ngModule: TbCoreModule,
            providers: [TB_SERVICES, TB_GUARDS]
        };
    }
}
TbCoreModule.decorators = [
    { type: NgModule, args: [{
                declarations: [TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS],
                exports: [TB_CONTAINERS, TB_COMPONENTS, TB_CONTROLS],
                providers: [TB_SERVICES, TB_GUARDS]
            },] },
];
/**
 * @nocollapse
 */
TbCoreModule.ctorParameters = () => [
    { type: TbCoreModule, decorators: [{ type: Optional }, { type: SkipSelf },] },
];
function TbCoreModule_tsickle_Closure_declarations() {
    /** @type {?} */
    TbCoreModule.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TbCoreModule.ctorParameters;
}
