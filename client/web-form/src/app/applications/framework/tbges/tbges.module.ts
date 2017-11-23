import { IDD_WIZARD_FRAMEComponent, IDD_WIZARD_FRAMEFactoryComponent } from './tbges/IDD_WIZARD_FRAME.component';
import { IDD_SLAVE_FRAMEComponent, IDD_SLAVE_FRAMEFactoryComponent } from './tbges/IDD_SLAVE_FRAME.component';
import { IDD_MASTER_FRAMEComponent, IDD_MASTER_FRAMEFactoryComponent } from './tbges/IDD_MASTER_FRAME.component';
import { IDD_FINDER_FRAMEComponent, IDD_FINDER_FRAMEFactoryComponent } from './tbges/IDD_FINDER_FRAME.component';
import { IDD_BATCH_FRAMEComponent, IDD_BATCH_FRAMEFactoryComponent } from './tbges/IDD_BATCH_FRAME.component';
import { IDD_TB_BASE_NAVIGATION_FRAMEComponent, IDD_TB_BASE_NAVIGATION_FRAMEFactoryComponent } from './tbbasenavigation/IDD_TB_BASE_NAVIGATION_FRAME.component';
import { IDD_TB_ACTIVITY_FRAMEComponent, IDD_TB_ACTIVITY_FRAMEFactoryComponent } from './tbactivitydocument/IDD_TB_ACTIVITY_FRAME.component';
import { IDD_BSP_FRAMEComponent, IDD_BSP_FRAMEFactoryComponent } from './bsp/IDD_BSP_FRAME.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_WIZARD_FRAME', component: IDD_WIZARD_FRAMEFactoryComponent },
            { path: 'IDD_SLAVE_FRAME', component: IDD_SLAVE_FRAMEFactoryComponent },
            { path: 'IDD_MASTER_FRAME', component: IDD_MASTER_FRAMEFactoryComponent },
            { path: 'IDD_FINDER_FRAME', component: IDD_FINDER_FRAMEFactoryComponent },
            { path: 'IDD_BATCH_FRAME', component: IDD_BATCH_FRAMEFactoryComponent },
            { path: 'IDD_TB_BASE_NAVIGATION_FRAME', component: IDD_TB_BASE_NAVIGATION_FRAMEFactoryComponent },
            { path: 'IDD_TB_ACTIVITY_FRAME', component: IDD_TB_ACTIVITY_FRAMEFactoryComponent },
            { path: 'IDD_BSP_FRAME', component: IDD_BSP_FRAMEFactoryComponent },
        ])],
    declarations: [
            IDD_WIZARD_FRAMEComponent, IDD_WIZARD_FRAMEFactoryComponent,
            IDD_SLAVE_FRAMEComponent, IDD_SLAVE_FRAMEFactoryComponent,
            IDD_MASTER_FRAMEComponent, IDD_MASTER_FRAMEFactoryComponent,
            IDD_FINDER_FRAMEComponent, IDD_FINDER_FRAMEFactoryComponent,
            IDD_BATCH_FRAMEComponent, IDD_BATCH_FRAMEFactoryComponent,
            IDD_TB_BASE_NAVIGATION_FRAMEComponent, IDD_TB_BASE_NAVIGATION_FRAMEFactoryComponent,
            IDD_TB_ACTIVITY_FRAMEComponent, IDD_TB_ACTIVITY_FRAMEFactoryComponent,
            IDD_BSP_FRAMEComponent, IDD_BSP_FRAMEFactoryComponent,
    ],
    exports: [
            IDD_WIZARD_FRAMEFactoryComponent,
            IDD_SLAVE_FRAMEFactoryComponent,
            IDD_MASTER_FRAMEFactoryComponent,
            IDD_FINDER_FRAMEFactoryComponent,
            IDD_BATCH_FRAMEFactoryComponent,
            IDD_TB_BASE_NAVIGATION_FRAMEFactoryComponent,
            IDD_TB_ACTIVITY_FRAMEFactoryComponent,
            IDD_BSP_FRAMEFactoryComponent,
    ],
    entryComponents: [
            IDD_WIZARD_FRAMEComponent,
            IDD_SLAVE_FRAMEComponent,
            IDD_MASTER_FRAMEComponent,
            IDD_FINDER_FRAMEComponent,
            IDD_BATCH_FRAMEComponent,
            IDD_TB_BASE_NAVIGATION_FRAMEComponent,
            IDD_TB_ACTIVITY_FRAMEComponent,
            IDD_BSP_FRAMEComponent,
    ]
})


export class TbGesModule { };