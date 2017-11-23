import { IDD_RECLASS_UPGRADEComponent, IDD_RECLASS_UPGRADEFactoryComponent } from './reclassificationsupgrade/IDD_RECLASS_UPGRADE.component';
import { IDD_PRINTFILEFORBEOComponent, IDD_PRINTFILEFORBEOFactoryComponent } from './printfileforbeo/IDD_PRINTFILEFORBEO.component';
import { IDD_TRANSCOD_EUComponent, IDD_TRANSCOD_EUFactoryComponent } from './balancereclassifications/IDD_TRANSCOD_EU.component';
import { IDD_RICLASS_COPYComponent, IDD_RICLASS_COPYFactoryComponent } from './balancereclassifications/IDD_RICLASS_COPY.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_RECLASS_UPGRADE', component: IDD_RECLASS_UPGRADEFactoryComponent },
            { path: 'IDD_PRINTFILEFORBEO', component: IDD_PRINTFILEFORBEOFactoryComponent },
            { path: 'IDD_TRANSCOD_EU', component: IDD_TRANSCOD_EUFactoryComponent },
            { path: 'IDD_RICLASS_COPY', component: IDD_RICLASS_COPYFactoryComponent },
        ])],
    declarations: [
            IDD_RECLASS_UPGRADEComponent, IDD_RECLASS_UPGRADEFactoryComponent,
            IDD_PRINTFILEFORBEOComponent, IDD_PRINTFILEFORBEOFactoryComponent,
            IDD_TRANSCOD_EUComponent, IDD_TRANSCOD_EUFactoryComponent,
            IDD_RICLASS_COPYComponent, IDD_RICLASS_COPYFactoryComponent,
    ],
    exports: [
            IDD_RECLASS_UPGRADEFactoryComponent,
            IDD_PRINTFILEFORBEOFactoryComponent,
            IDD_TRANSCOD_EUFactoryComponent,
            IDD_RICLASS_COPYFactoryComponent,
    ],
    entryComponents: [
            IDD_RECLASS_UPGRADEComponent,
            IDD_PRINTFILEFORBEOComponent,
            IDD_TRANSCOD_EUComponent,
            IDD_RICLASS_COPYComponent,
    ]
})


export class BalanceAnalysisModule { };