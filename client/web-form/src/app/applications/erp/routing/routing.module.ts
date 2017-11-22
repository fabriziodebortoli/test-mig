import { IDD_WC_FAMILYComponent, IDD_WC_FAMILYFactoryComponent } from './wcfamilies/IDD_WC_FAMILY.component';
import { IDD_WORKCENTERSComponent, IDD_WORKCENTERSFactoryComponent } from './wc/IDD_WORKCENTERS.component';
import { IDD_OPERATIONSComponent, IDD_OPERATIONSFactoryComponent } from './operations/IDD_OPERATIONS.component';
import { IDD_FACTORIESComponent, IDD_FACTORIESFactoryComponent } from './factories/IDD_FACTORIES.component';
import { IDD_DRAWINGComponent, IDD_DRAWINGFactoryComponent } from './drawings/IDD_DRAWING.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_WC_FAMILY', component: IDD_WC_FAMILYFactoryComponent },
            { path: 'IDD_WORKCENTERS', component: IDD_WORKCENTERSFactoryComponent },
            { path: 'IDD_OPERATIONS', component: IDD_OPERATIONSFactoryComponent },
            { path: 'IDD_FACTORIES', component: IDD_FACTORIESFactoryComponent },
            { path: 'IDD_DRAWING', component: IDD_DRAWINGFactoryComponent },
        ])],
    declarations: [
            IDD_WC_FAMILYComponent, IDD_WC_FAMILYFactoryComponent,
            IDD_WORKCENTERSComponent, IDD_WORKCENTERSFactoryComponent,
            IDD_OPERATIONSComponent, IDD_OPERATIONSFactoryComponent,
            IDD_FACTORIESComponent, IDD_FACTORIESFactoryComponent,
            IDD_DRAWINGComponent, IDD_DRAWINGFactoryComponent,
    ],
    exports: [
            IDD_WC_FAMILYFactoryComponent,
            IDD_WORKCENTERSFactoryComponent,
            IDD_OPERATIONSFactoryComponent,
            IDD_FACTORIESFactoryComponent,
            IDD_DRAWINGFactoryComponent,
    ],
    entryComponents: [
            IDD_WC_FAMILYComponent,
            IDD_WORKCENTERSComponent,
            IDD_OPERATIONSComponent,
            IDD_FACTORIESComponent,
            IDD_DRAWINGComponent,
    ]
})


export class RoutingModule { };