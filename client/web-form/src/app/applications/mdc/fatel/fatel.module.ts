import { IDD_FATEL_ADDITIONALDATAComponent, IDD_FATEL_ADDITIONALDATAFactoryComponent } from './fateladditionaldata/IDD_FATEL_ADDITIONALDATA.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_FATEL_ADDITIONALDATA', component: IDD_FATEL_ADDITIONALDATAFactoryComponent },
        ])],
    declarations: [
            IDD_FATEL_ADDITIONALDATAComponent, IDD_FATEL_ADDITIONALDATAFactoryComponent,
    ],
    exports: [
            IDD_FATEL_ADDITIONALDATAFactoryComponent,
    ],
    entryComponents: [
            IDD_FATEL_ADDITIONALDATAComponent,
    ]
})


export class FATELModule { };