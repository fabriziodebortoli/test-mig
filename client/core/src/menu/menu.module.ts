import { TbSharedModule } from './../shared/shared.module';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { Logger } from './../core/services/logger.service';

import { LoginComponent } from './components/login/login.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { SearchComponent } from './components/menu/search/search.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuComponent } from './components/menu/menu.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';


export { LoginComponent } from './components/login/login.component';
export { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
export { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
export { FavoritesComponent } from './components/menu/favorites/favorites.component';
export { SearchComponent } from './components/menu/search/search.component';
export { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
export { MenuComponent } from './components/menu/menu.component';
export { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
export { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
export { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
export { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
export { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
export { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
export { MostUsedComponent } from './components/menu/most-used/most-used.component';



// import { MenuComponent } from './components/menu/menu.component';

import { MenuService } from './services/menu.service';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';
import { LocalizationService } from './services/localization.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';

//WARNING: put here only GLOBAL services, NOT component level services
export const TB_MENU_SERVICES = [
    MenuService,
    ImageService,
    HttpMenuService,
    SettingsService,
    LocalizationService,
    EventManagerService
];

export { MenuService } from './services/menu.service';
export { ImageService } from './services/image.service';
export { HttpMenuService } from './services/http-menu.service';
export { LocalizationService } from './services/localization.service';
export { SettingsService } from './services/settings.service';
export { EventManagerService } from './services/event-manager.service';

import { LayoutModule } from '@progress/kendo-angular-layout';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { DialogModule } from '@progress/kendo-angular-dialog';

@NgModule({
    imports: [
        // CommonModule,
        // FormsModule,
        // ReactiveFormsModule,
        TbSharedModule,
        LayoutModule,
        ButtonsModule,
        DialogModule
    ],

    declarations: [
        LoginComponent,
        ApplicationSelectorComponent,
        FavoritesComponent,
        MostUsedComponent,
        GroupSelectorComponent,
        MenuContainerComponent,
        MenuContentComponent,
        MenuElementComponent,
        MenuStepperComponent,
        ConnectionInfoDialogComponent,
        SearchComponent,
        MenuComponent,
        MenuTabberComponent,
        MenuTabComponent,
    ],
    exports: [
        LoginComponent,
        ApplicationSelectorComponent,
        FavoritesComponent,
        MostUsedComponent,
        GroupSelectorComponent,
        MenuContainerComponent,
        MenuContentComponent,
        MenuElementComponent,
        MenuStepperComponent,
        SearchComponent,
        MenuComponent,
    ],
    providers: [TB_MENU_SERVICES],
    entryComponents: [
        ConnectionInfoDialogComponent
    ]
})
export class TbMenuModule {
    // static forRoot(): ModuleWithProviders {
    //     return {
    //         ngModule: TbMenuModule,
    //         providers: [TB_MENU_SERVICES]
    //     };
    // }

    constructor(public logger: Logger) {
        this.logger.debug('TbMenuModule from Core instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
}

