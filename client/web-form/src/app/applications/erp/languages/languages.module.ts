import { IDD_LANGUAGES_FRAMEComponent, IDD_LANGUAGES_FRAMEFactoryComponent } from './languages/IDD_LANGUAGES_FRAME.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_LANGUAGES_FRAME', component: IDD_LANGUAGES_FRAMEFactoryComponent },
        ])],
    declarations: [
            IDD_LANGUAGES_FRAMEComponent, IDD_LANGUAGES_FRAMEFactoryComponent,
    ],
    exports: [
            IDD_LANGUAGES_FRAMEFactoryComponent,
    ],
    entryComponents: [
            IDD_LANGUAGES_FRAMEComponent,
    ]
})


export class LanguagesModule { };