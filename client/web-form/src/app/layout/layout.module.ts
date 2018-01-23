import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { DocumentLayoutComponent, DocumentLayoutFactoryComponent } from './document/document-layout.component';
import { DocumentMenuComponent, DocumentMenuFactoryComponent } from './document-menu/document-menu.component';
import { SharedModule } from './../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'document', component: DocumentLayoutFactoryComponent },
            { path: 'document-menu', component: DocumentMenuFactoryComponent }
        ])],
    declarations: [
            DocumentLayoutComponent, DocumentLayoutFactoryComponent,
            DocumentMenuComponent, DocumentMenuFactoryComponent
    ],
    exports: [
            DocumentLayoutFactoryComponent,DocumentMenuFactoryComponent
    ],
    entryComponents: [
            DocumentLayoutComponent,DocumentMenuComponent
    ]
})


export class LayoutModule { };