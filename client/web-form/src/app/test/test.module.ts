import { FormsModule } from '@angular/forms';

import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { SharedModule } from './../shared/shared.module';

import { DataServiceComponent, DataServiceFactoryComponent } from './data-service/data-service.component';
import { ExplorerComponent, ExplorerFactoryComponent } from './explorer/explorer.component';

@NgModule({
    imports: [
        FormsModule,
        SharedModule,
        RouterModule.forChild([
            { path: 'dataservice', component: DataServiceFactoryComponent },
            { path: 'explorer', component: ExplorerFactoryComponent },
        ])],
    declarations: [
        DataServiceComponent, DataServiceFactoryComponent, ExplorerComponent, ExplorerFactoryComponent
    ],
    entryComponents: [DataServiceComponent, ExplorerComponent]
})
export class TestModule { }