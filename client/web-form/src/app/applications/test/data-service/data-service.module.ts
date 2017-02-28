import { RouterModule } from '@angular/router';
import { SharedModule } from './../../../shared/shared.module';

import { NgModule } from '@angular/core';
import { DataServiceComponent, DataServiceFactoryComponent } from './data-service.component';


@NgModule({
imports: [
        SharedModule,
        RouterModule.forChild([
            { path: 'dataservice', component: DataServiceFactoryComponent },
        ])],
    declarations: [
        DataServiceComponent, DataServiceFactoryComponent
    ],
    entryComponents:[DataServiceComponent]
})


export class DataServiceModule { }