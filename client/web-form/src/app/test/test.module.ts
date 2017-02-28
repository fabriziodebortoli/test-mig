import { SharedModule } from './../shared/shared.module';
import { RouterModule } from '@angular/router';

import { NgModule } from '@angular/core';
import { DataServiceComponent, DataServiceFactoryComponent } from './data-service/data-service.component';


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


export class TestModule { }