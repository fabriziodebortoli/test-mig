import { IDD_MANAGEFILE_WIZARD_MASTERComponent, IDD_MANAGEFILE_WIZARD_MASTERFactoryComponent } from './tbmanagefile/IDD_MANAGEFILE_WIZARD_MASTER.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_MANAGEFILE_WIZARD_MASTER', component: IDD_MANAGEFILE_WIZARD_MASTERFactoryComponent },
        ])],
    declarations: [
            IDD_MANAGEFILE_WIZARD_MASTERComponent, IDD_MANAGEFILE_WIZARD_MASTERFactoryComponent,
    ],
    exports: [
            IDD_MANAGEFILE_WIZARD_MASTERFactoryComponent,
    ],
    entryComponents: [
            IDD_MANAGEFILE_WIZARD_MASTERComponent,
    ]
})


export class TbGenlibUIModule { };