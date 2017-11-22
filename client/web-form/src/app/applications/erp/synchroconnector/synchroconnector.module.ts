import { IDD_MASSIVE_VALIDATIONComponent, IDD_MASSIVE_VALIDATIONFactoryComponent } from './infinitymassivevalidation/IDD_MASSIVE_VALIDATION.component';
import { IDD_CRM_INFINITY_MASSIVE_SYNCHROComponent, IDD_CRM_INFINITY_MASSIVE_SYNCHROFactoryComponent } from './uicrmmassivesynchro/IDD_CRM_INFINITY_MASSIVE_SYNCHRO.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_MASSIVE_VALIDATION', component: IDD_MASSIVE_VALIDATIONFactoryComponent },
            { path: 'IDD_CRM_INFINITY_MASSIVE_SYNCHRO', component: IDD_CRM_INFINITY_MASSIVE_SYNCHROFactoryComponent },
        ])],
    declarations: [
            IDD_MASSIVE_VALIDATIONComponent, IDD_MASSIVE_VALIDATIONFactoryComponent,
            IDD_CRM_INFINITY_MASSIVE_SYNCHROComponent, IDD_CRM_INFINITY_MASSIVE_SYNCHROFactoryComponent,
    ],
    exports: [
            IDD_MASSIVE_VALIDATIONFactoryComponent,
            IDD_CRM_INFINITY_MASSIVE_SYNCHROFactoryComponent,
    ],
    entryComponents: [
            IDD_MASSIVE_VALIDATIONComponent,
            IDD_CRM_INFINITY_MASSIVE_SYNCHROComponent,
    ]
})


export class SynchroConnectorModule { };