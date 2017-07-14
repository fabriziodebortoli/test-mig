import { FormsModule } from '@angular/forms';

import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { TbSharedModule } from './../shared/shared.module';
import { CommonModule } from '@angular/common';

import { DataServiceComponent, DataServiceFactoryComponent } from './data-service/data-service.component';
import { ExplorerComponent, ExplorerFactoryComponent } from './explorer/explorer.component';
import { GridTestComponent, GridTestFactoryComponent } from './grid-test/grid-test.component';
import { IconsTestComponent, IconsTestFactoryComponent } from './icons-test/icons-test.component';
import { RadarTestComponent, RadarTestFactoryComponent } from './radar-test/radar-test.component';


@NgModule({
    imports: [
        CommonModule,
        TbSharedModule,
        RouterModule.forChild([
            { path: 'dataservice', component: DataServiceFactoryComponent },
            { path: 'explorer', component: ExplorerFactoryComponent },
            { path: 'grid', component: GridTestFactoryComponent },
            { path: 'icons', component: IconsTestFactoryComponent },
            { path: 'radar', component: RadarTestFactoryComponent },
        ]),
        // IconsModule
    ],
    declarations: [
        DataServiceComponent, DataServiceFactoryComponent,
        ExplorerComponent, ExplorerFactoryComponent,
        GridTestComponent, GridTestFactoryComponent,
        IconsTestComponent, IconsTestFactoryComponent
        RadarTestComponent, RadarTestFactoryComponent
    ],
    entryComponents: [DataServiceComponent, GridTestComponent, ExplorerComponent, IconsTestComponent, RadarTestComponent]
})
export class TbTestModule { }