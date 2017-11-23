import { IDD_ITEMSCODEComponent, IDD_ITEMSCODEFactoryComponent } from './itemcodes/IDD_ITEMSCODE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_ITEMSCODE', component: IDD_ITEMSCODEFactoryComponent },
        ])],
    declarations: [
            IDD_ITEMSCODEComponent, IDD_ITEMSCODEFactoryComponent,
    ],
    exports: [
            IDD_ITEMSCODEFactoryComponent,
    ],
    entryComponents: [
            IDD_ITEMSCODEComponent,
    ]
})


export class ItemCodesModule { };