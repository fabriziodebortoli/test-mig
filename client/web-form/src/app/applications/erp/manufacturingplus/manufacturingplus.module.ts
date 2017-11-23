import { IDD_CONTROL_PANEL_FINDComponent, IDD_CONTROL_PANEL_FINDFactoryComponent } from './controlpanel/IDD_CONTROL_PANEL_FIND.component';
import { IDD_CONTROL_PANELComponent, IDD_CONTROL_PANELFactoryComponent } from './controlpanel/IDD_CONTROL_PANEL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_CONTROL_PANEL_FIND', component: IDD_CONTROL_PANEL_FINDFactoryComponent },
            { path: 'IDD_CONTROL_PANEL', component: IDD_CONTROL_PANELFactoryComponent },
        ])],
    declarations: [
            IDD_CONTROL_PANEL_FINDComponent, IDD_CONTROL_PANEL_FINDFactoryComponent,
            IDD_CONTROL_PANELComponent, IDD_CONTROL_PANELFactoryComponent,
    ],
    exports: [
            IDD_CONTROL_PANEL_FINDFactoryComponent,
            IDD_CONTROL_PANELFactoryComponent,
    ],
    entryComponents: [
            IDD_CONTROL_PANEL_FINDComponent,
            IDD_CONTROL_PANELComponent,
    ]
})


export class ManufacturingPlusModule { };