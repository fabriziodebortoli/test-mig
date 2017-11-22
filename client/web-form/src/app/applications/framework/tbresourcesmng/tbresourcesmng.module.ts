import { IDD_WORKER_WINDOWComponent, IDD_WORKER_WINDOWFactoryComponent } from './workerwindow/IDD_WORKER_WINDOW.component';
import { IDD_WORKERSComponent, IDD_WORKERSFactoryComponent } from './workers/IDD_WORKERS.component';
import { IDD_RESOURCETYPESComponent, IDD_RESOURCETYPESFactoryComponent } from './resourcetypes/IDD_RESOURCETYPES.component';
import { IDD_RESOURCES_LAYOUTComponent, IDD_RESOURCES_LAYOUTFactoryComponent } from './resourceslayout/IDD_RESOURCES_LAYOUT.component';
import { IDD_RESOURCESComponent, IDD_RESOURCESFactoryComponent } from './resources/IDD_RESOURCES.component';
import { IDD_CALENDARSComponent, IDD_CALENDARSFactoryComponent } from './calendars/IDD_CALENDARS.component';
import { IDD_ARRANGEMENTSComponent, IDD_ARRANGEMENTSFactoryComponent } from './arrangements/IDD_ARRANGEMENTS.component';
import { IDD_ABSENCEREASONSComponent, IDD_ABSENCEREASONSFactoryComponent } from './absencereasons/IDD_ABSENCEREASONS.component';
import { IDD_RESOURCES_LAYOUT_ACTIONComponent, IDD_RESOURCES_LAYOUT_ACTIONFactoryComponent } from './resourceslayoutaction/IDD_RESOURCES_LAYOUT_ACTION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_WORKER_WINDOW', component: IDD_WORKER_WINDOWFactoryComponent },
            { path: 'IDD_WORKERS', component: IDD_WORKERSFactoryComponent },
            { path: 'IDD_RESOURCETYPES', component: IDD_RESOURCETYPESFactoryComponent },
            { path: 'IDD_RESOURCES_LAYOUT', component: IDD_RESOURCES_LAYOUTFactoryComponent },
            { path: 'IDD_RESOURCES', component: IDD_RESOURCESFactoryComponent },
            { path: 'IDD_CALENDARS', component: IDD_CALENDARSFactoryComponent },
            { path: 'IDD_ARRANGEMENTS', component: IDD_ARRANGEMENTSFactoryComponent },
            { path: 'IDD_ABSENCEREASONS', component: IDD_ABSENCEREASONSFactoryComponent },
            { path: 'IDD_RESOURCES_LAYOUT_ACTION', component: IDD_RESOURCES_LAYOUT_ACTIONFactoryComponent },
        ])],
    declarations: [
            IDD_WORKER_WINDOWComponent, IDD_WORKER_WINDOWFactoryComponent,
            IDD_WORKERSComponent, IDD_WORKERSFactoryComponent,
            IDD_RESOURCETYPESComponent, IDD_RESOURCETYPESFactoryComponent,
            IDD_RESOURCES_LAYOUTComponent, IDD_RESOURCES_LAYOUTFactoryComponent,
            IDD_RESOURCESComponent, IDD_RESOURCESFactoryComponent,
            IDD_CALENDARSComponent, IDD_CALENDARSFactoryComponent,
            IDD_ARRANGEMENTSComponent, IDD_ARRANGEMENTSFactoryComponent,
            IDD_ABSENCEREASONSComponent, IDD_ABSENCEREASONSFactoryComponent,
            IDD_RESOURCES_LAYOUT_ACTIONComponent, IDD_RESOURCES_LAYOUT_ACTIONFactoryComponent,
    ],
    exports: [
            IDD_WORKER_WINDOWFactoryComponent,
            IDD_WORKERSFactoryComponent,
            IDD_RESOURCETYPESFactoryComponent,
            IDD_RESOURCES_LAYOUTFactoryComponent,
            IDD_RESOURCESFactoryComponent,
            IDD_CALENDARSFactoryComponent,
            IDD_ARRANGEMENTSFactoryComponent,
            IDD_ABSENCEREASONSFactoryComponent,
            IDD_RESOURCES_LAYOUT_ACTIONFactoryComponent,
    ],
    entryComponents: [
            IDD_WORKER_WINDOWComponent,
            IDD_WORKERSComponent,
            IDD_RESOURCETYPESComponent,
            IDD_RESOURCES_LAYOUTComponent,
            IDD_RESOURCESComponent,
            IDD_CALENDARSComponent,
            IDD_ARRANGEMENTSComponent,
            IDD_ABSENCEREASONSComponent,
            IDD_RESOURCES_LAYOUT_ACTIONComponent,
    ]
})


export class TbResourcesMngModule { };