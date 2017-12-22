import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { DocumentLayoutComponent, DocumentLayoutFactoryComponent } from './document/document-layout.component';
import { SharedModule } from './../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'document', component: DocumentLayoutFactoryComponent },
        ])],
    declarations: [
            DocumentLayoutComponent, DocumentLayoutFactoryComponent,
    ],
    exports: [
            DocumentLayoutFactoryComponent,
    ],
    entryComponents: [
            DocumentLayoutComponent,
    ]
})


export class LayoutModule { };