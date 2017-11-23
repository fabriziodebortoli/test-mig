import { IDD_VISUALIZERComponent, IDD_VISUALIZERFactoryComponent } from './visualizerxbrlinstance/IDD_VISUALIZER.component';
import { IDD_UPGRADEComponent, IDD_UPGRADEFactoryComponent } from './upgradetaxonomies/IDD_UPGRADE.component';
import { IDD_IMPORTComponent, IDD_IMPORTFactoryComponent } from './importxbrlreclassifications/IDD_IMPORT.component';
import { IDD_CREATEComponent, IDD_CREATEFactoryComponent } from './createxbrlreclassifications/IDD_CREATE.component';
import { IDD_CREATE_XBRLComponent, IDD_CREATE_XBRLFactoryComponent } from './createxbrlinstance/IDD_CREATE_XBRL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_VISUALIZER', component: IDD_VISUALIZERFactoryComponent },
            { path: 'IDD_UPGRADE', component: IDD_UPGRADEFactoryComponent },
            { path: 'IDD_IMPORT', component: IDD_IMPORTFactoryComponent },
            { path: 'IDD_CREATE', component: IDD_CREATEFactoryComponent },
            { path: 'IDD_CREATE_XBRL', component: IDD_CREATE_XBRLFactoryComponent },
        ])],
    declarations: [
            IDD_VISUALIZERComponent, IDD_VISUALIZERFactoryComponent,
            IDD_UPGRADEComponent, IDD_UPGRADEFactoryComponent,
            IDD_IMPORTComponent, IDD_IMPORTFactoryComponent,
            IDD_CREATEComponent, IDD_CREATEFactoryComponent,
            IDD_CREATE_XBRLComponent, IDD_CREATE_XBRLFactoryComponent,
    ],
    exports: [
            IDD_VISUALIZERFactoryComponent,
            IDD_UPGRADEFactoryComponent,
            IDD_IMPORTFactoryComponent,
            IDD_CREATEFactoryComponent,
            IDD_CREATE_XBRLFactoryComponent,
    ],
    entryComponents: [
            IDD_VISUALIZERComponent,
            IDD_UPGRADEComponent,
            IDD_IMPORTComponent,
            IDD_CREATEComponent,
            IDD_CREATE_XBRLComponent,
    ]
})


export class XBRLModule { };