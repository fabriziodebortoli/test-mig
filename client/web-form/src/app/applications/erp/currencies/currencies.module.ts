import { IDD_FDLComponent, IDD_FDLFactoryComponent } from './fixingdownload/IDD_FDL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_FDL', component: IDD_FDLFactoryComponent },
        ])],
    declarations: [
            IDD_FDLComponent, IDD_FDLFactoryComponent,
    ],
    exports: [
            IDD_FDLFactoryComponent,
    ],
    entryComponents: [
            IDD_FDLComponent,
    ]
})


export class CurrenciesModule { };