import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from './../shared/shared.module';

import { LoginComponent } from './components/login/login.component';
import { LogoutComponent } from './components/logout/logout.component';

export { LoginComponent } from './components/login/login.component';
export { LogoutComponent } from './components/logout/logout.component';

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
    ]
})
export class TbMenuModule { }

