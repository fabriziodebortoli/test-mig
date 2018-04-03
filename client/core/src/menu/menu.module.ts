import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from './../shared/shared.module';
import { Logger } from './../core/services/logger.service';

import { LoginComponent } from './components/login/login.component';
import { LogoutComponent } from './components/logout/logout.component';


export { LoginComponent } from './components/login/login.component';
export { LogoutComponent } from './components/logout/logout.component';


import { MenuService } from './services/menu.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';


//WARNING: put here only GLOBAL services, NOT component level services
export const TB_MENU_SERVICES = [
    MenuService,
    ImageService,
    HttpMenuService
];

export { MenuService } from './services/menu.service';
export { ImageService } from './services/image.service';
export { HttpMenuService } from './services/http-menu.service';

const NG_MODULES = [
    CommonModule,
    ReactiveFormsModule,
    FormsModule
];

@NgModule({
    imports: [
        NG_MODULES,
        TbSharedModule,
        TbIconsModule
    ],

    declarations: [
        LoginComponent,
        LogoutComponent
    ],
    exports: [
        LoginComponent,
        LogoutComponent
    ],
    providers: [TB_MENU_SERVICES]
})
export class TbMenuModule { }

