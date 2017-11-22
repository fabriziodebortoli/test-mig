import { IDD_STORAGE_ENTRIESComponent, IDD_STORAGE_ENTRIESFactoryComponent } from './storagesentries/IDD_STORAGE_ENTRIES.component';
import { IDD_STORAGEINVComponent, IDD_STORAGEINVFactoryComponent } from './storages/IDD_STORAGEINV.component';
import { IDD_NAMESTORAGEComponent, IDD_NAMESTORAGEFactoryComponent } from './storagegroups/IDD_NAMESTORAGE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_STORAGE_ENTRIES', component: IDD_STORAGE_ENTRIESFactoryComponent },
            { path: 'IDD_STORAGEINV', component: IDD_STORAGEINVFactoryComponent },
            { path: 'IDD_NAMESTORAGE', component: IDD_NAMESTORAGEFactoryComponent },
        ])],
    declarations: [
            IDD_STORAGE_ENTRIESComponent, IDD_STORAGE_ENTRIESFactoryComponent,
            IDD_STORAGEINVComponent, IDD_STORAGEINVFactoryComponent,
            IDD_NAMESTORAGEComponent, IDD_NAMESTORAGEFactoryComponent,
    ],
    exports: [
            IDD_STORAGE_ENTRIESFactoryComponent,
            IDD_STORAGEINVFactoryComponent,
            IDD_NAMESTORAGEFactoryComponent,
    ],
    entryComponents: [
            IDD_STORAGE_ENTRIESComponent,
            IDD_STORAGEINVComponent,
            IDD_NAMESTORAGEComponent,
    ]
})


export class MultiStorageModule { };