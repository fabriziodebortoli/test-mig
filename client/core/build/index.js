import { NgModule } from '@angular/core';
/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule } from './core/core.module';
export { TbCoreModule, BOService, BOClient, BOHelperService, ComponentService, ComponentCreatedArgs, DataService, DocumentService, EnumsService, EventDataService, ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, SidenavService, TabberService, UtilsService, WebSocketService, SocketMessage, CoreGuard, FrameComponent, FrameContentComponent, ViewComponent, ViewContainerComponent, DockpaneComponent, DockpaneContainerComponent, TileManagerComponent, TileGroupComponent, TileComponent, TilePanelComponent, LayoutContainerComponent, MessageDialogComponent, MessageDlgArgs, MessageDlgResult, BOComponent, DocumentComponent, DynamicCmpComponent, PageNotFoundComponent, TbComponent, TopbarComponent, TopbarMenuComponent, TopbarMenuAppComponent, TopbarMenuElementsComponent, TopbarMenuTestComponent, TopbarMenuUserComponent, ContextMenuComponent, SaveComponent, OpenComponent, AccordionComponent, AccordionGroupComponent, HeaderStripComponent, ToolbarTopComponent, ToolbarSeparatorComponent, ToolbarTopButtonComponent, ToolbarBottomComponent, ToolbarBottomButtonComponent, BodyEditComponent, BoolEditComponent, ButtonComponent, CaptionComponent, LinearGaugeComponent, CheckBoxComponent, ColorPickerComponent, ComboComponent, ComboSimpleComponent, DateInputComponent, EmailComponent, EnumComboComponent, FileComponent, GridComponent, ImageComponent, LabelStaticComponent, LinkComponent, MaskedTextBoxComponent, NumericTextBoxComponent, PasswordComponent, PhoneComponent, PlaceholderComponent, RadioComponent, SectionTitleComponent, StateButtonComponent, TbCardComponent, TbCardContentComponent, TbCardTitleComponent, TextComponent, TextareaComponent, TimeInputComponent, UnknownComponent } from './core/core.module';
/**
 * Modulo Shared con tutti i moduli condivisi (Common, Form, Router, Material, Kendo)
 */
import { TbSharedModule } from './shared/shared.module';
export { TbSharedModule, ContextMenuDirective, StateButtonDirective, LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective, TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './shared/shared.module';
/**
 * Modulo Menu
 */
import { TbMenuModule } from './menu/menu.module';
export { TbMenuModule, MenuService, EventManagerService, SettingsService, HttpMenuService, ImageService, LocalizationService, GroupSelectorComponent, MenuStepperComponent, ConnectionInfoDialogComponent, ProductInfoDialogComponent, SearchComponent, MostUsedComponent, MenuComponent, ApplicationSelectorComponent, FavoritesComponent, LoginComponent, MenuTabberComponent, MenuTabComponent, MenuContainerComponent, MenuElementComponent, MenuContentComponent } from './menu/menu.module';
/**
 * Modulo Icon Font
 */
import { TbIconsModule } from './icons/icons.module';
export { TbIconsModule, TbIconComponent, M4IconComponent } from './icons/icons.module';
const /** @type {?} */ TB_MODULES = [
    TbCoreModule,
    TbSharedModule,
    TbMenuModule,
    TbIconsModule
];
/**
 * Modulo principale della libreria ngTaskbuilder
 */
export class TaskbuilderCoreModule {
    /**
     * @return {?}
     */
    static forRoot() {
        return {
            ngModule: TaskbuilderCoreModule,
        };
    }
}
TaskbuilderCoreModule.decorators = [
    { type: NgModule, args: [{
                imports: [TB_MODULES],
                exports: [TB_MODULES],
            },] },
];
/**
 * @nocollapse
 */
TaskbuilderCoreModule.ctorParameters = () => [];
function TaskbuilderCoreModule_tsickle_Closure_declarations() {
    /** @type {?} */
    TaskbuilderCoreModule.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TaskbuilderCoreModule.ctorParameters;
}
