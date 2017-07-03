import { TbSharedModule } from './../shared/shared.module';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { Logger } from './../core/services/logger.service';

import {
    LoginComponent, ApplicationSelectorComponent, FavoritesComponent, MostUsedComponent, GroupSelectorComponent,
    MenuContainerComponent, MenuContentComponent, MenuElementComponent, MenuStepperComponent,
    ConnectionInfoDialogComponent, ProductInfoDialogComponent, SearchComponent
} from './components';
export * from './components';

// import { MenuComponent } from './components/menu/menu.component';

// import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
// import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
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

export * from './services/menu.service';
export * from './services/image.service';
export * from './services/http-menu.service';
export * from './services/localization.service';
export * from './services/settings.service';
export * from './services/event-manager.service';

import { LayoutModule } from '@progress/kendo-angular-layout';
import { ButtonsModule } from '@progress/kendo-angular-buttons';

@NgModule({
    imports: [
        // CommonModule,
        // FormsModule,
        // ReactiveFormsModule,
        TbSharedModule,
        LayoutModule,
        ButtonsModule
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
        ProductInfoDialogComponent,
        ConnectionInfoDialogComponent,
        SearchComponent,
        // MenuComponent,
        // MenuTabberComponent,
        // MenuTabComponent,
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
        // MenuComponent,
    ],
    providers: [TB_MENU_SERVICES],
    //   entryComponents: [
    //     ProductInfoDialogComponent,
    //     ConnectionInfoDialogComponent
    //   ]
})
export class TbMenuModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: TbMenuModule,
            providers: [TB_MENU_SERVICES]
        };
    }

    constructor(private logger: Logger) {
        this.logger.debug('TbMenuModule from Core instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
}

