import { TbSharedModule } from './../shared/shared.module';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { Logger } from './../core/services/logger.service';

import { LoginComponent, ApplicationSelectorComponent, FavoritesComponent, MostUsedComponent } from './components';
export * from './components';

// import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
// import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
// import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
// import { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';

// import { SearchComponent } from './components/menu/search/search.component';
// import { MenuComponent } from './components/menu/menu.component';



// import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
// import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
// import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
// import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
// import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
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

@NgModule({
    imports: [
        // CommonModule,
        // FormsModule,
        // ReactiveFormsModule,
        TbSharedModule
    ],

    declarations: [
        LoginComponent,
        ApplicationSelectorComponent,
        FavoritesComponent,
        MostUsedComponent,
        // MenuComponent,
        // MenuContainerComponent,
        // SearchComponent,
        // ProductInfoDialogComponent,
        // ConnectionInfoDialogComponent,
        // GroupSelectorComponent,
        // MenuStepperComponent,
        // MenuTabberComponent,
        // MenuTabComponent,
        // MenuElementComponent,
        // MenuContentComponent
    ],
    exports: [
        LoginComponent,
        ApplicationSelectorComponent,
        FavoritesComponent,
        MostUsedComponent,

        // MenuComponent,
        // MenuElementComponent,
        // MenuContainerComponent,
        // SearchComponent,
        // GroupSelectorComponent,
        // MenuStepperComponent,
        // MenuContentComponent
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

