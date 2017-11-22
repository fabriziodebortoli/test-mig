import { IDD_DECHARGESPOLICIES_FULLComponent, IDD_DECHARGESPOLICIES_FULLFactoryComponent } from './chargepolicies/IDD_DECHARGESPOLICIES_FULL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_DECHARGESPOLICIES_FULL', component: IDD_DECHARGESPOLICIES_FULLFactoryComponent },
        ])],
    declarations: [
            IDD_DECHARGESPOLICIES_FULLComponent, IDD_DECHARGESPOLICIES_FULLFactoryComponent,
    ],
    exports: [
            IDD_DECHARGESPOLICIES_FULLFactoryComponent,
    ],
    entryComponents: [
            IDD_DECHARGESPOLICIES_FULLComponent,
    ]
})


export class ChargePoliciesModule { };