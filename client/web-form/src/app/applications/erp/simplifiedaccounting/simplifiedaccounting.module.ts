import { IDD_SACTOTComponent, IDD_SACTOTFactoryComponent } from './simplifiedaccountingtotals/IDD_SACTOT.component';
import { IDD_SACGRPComponent, IDD_SACGRPFactoryComponent } from './simplifiedaccountinggroups/IDD_SACGRP.component';
import { IDD_SACRENUMBERComponent, IDD_SACRENUMBERFactoryComponent } from './jerenumbering/IDD_SACRENUMBER.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SACTOT', component: IDD_SACTOTFactoryComponent },
            { path: 'IDD_SACGRP', component: IDD_SACGRPFactoryComponent },
            { path: 'IDD_SACRENUMBER', component: IDD_SACRENUMBERFactoryComponent },
        ])],
    declarations: [
            IDD_SACTOTComponent, IDD_SACTOTFactoryComponent,
            IDD_SACGRPComponent, IDD_SACGRPFactoryComponent,
            IDD_SACRENUMBERComponent, IDD_SACRENUMBERFactoryComponent,
    ],
    exports: [
            IDD_SACTOTFactoryComponent,
            IDD_SACGRPFactoryComponent,
            IDD_SACRENUMBERFactoryComponent,
    ],
    entryComponents: [
            IDD_SACTOTComponent,
            IDD_SACGRPComponent,
            IDD_SACRENUMBERComponent,
    ]
})


export class SimplifiedAccountingModule { };