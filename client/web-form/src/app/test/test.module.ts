import { FormsModule } from '@angular/forms';

import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { SharedModule } from './../shared/shared.module';
import { CommonModule } from '@angular/common';

import { DataServiceComponent, DataServiceFactoryComponent } from './data-service/data-service.component';
import { ExplorerComponent, ExplorerFactoryComponent } from './explorer/explorer.component';
import { GridTestComponent, GridTestFactoryComponent } from './grid-test/grid-test.component';
import { IconsTestComponent, IconsTestFactoryComponent } from './icons-test/icons-test.component';

// import { IconsModule } from '@taskbuilder/icons';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        RouterModule.forChild([
            { path: 'dataservice', component: DataServiceFactoryComponent },
            { path: 'explorer', component: ExplorerFactoryComponent },
            { path: 'grid', component: GridTestFactoryComponent },
            // { path: 'icons', component: IconsTestFactoryComponent },
        ]),
        // IconsModule
    ],
    declarations: [
        DataServiceComponent, DataServiceFactoryComponent,
        ExplorerComponent, ExplorerFactoryComponent,
        GridTestComponent, GridTestFactoryComponent,
        IconsTestComponent, IconsTestFactoryComponent
    ],
    entryComponents: [DataServiceComponent, ExplorerComponent, GridTestComponent, IconsTestComponent]
})
export class TestModule { }