import { Component, ViewContainerRef, OnInit, OnDestroy, ComponentRef, Input, ViewChild } from '@angular/core';
import { Subscription } from '../../rxjs.imports';

import { MessageDlgArgs, DiagnosticData } from './../models/message-dialog.model';
import { ComponentInfo } from './../models/component-info.model';

import { EventDataService } from './../../core/services/eventdata.service';
import { ComponentService } from './../../core/services/component.service';
import { Store } from './../../core/services/store.service';

import { DynamicDialogComponent } from './../containers/dynamic-dialog/dynamic-dialog.component';
import { DiagnosticDialogComponent } from './../containers/diagnostic-dialog/diagnostic-dialog.component';
import { MessageDialogComponent } from './../containers/message-dialog/message-dialog.component';
import { DocumentComponent } from './document.component';

/**ATTENZIONE! Questo componente serve per condividere una stesa istanza di EventDataService
 * fra più dynamic component appartenenti allo stesso contesto
 * (caso di view e slave view, oppure finestre di dialogo, che condividono lo stesso documento)
 */
@Component({
    selector: 'tb-dynamic-cmp-tree',
    template: '<ng-content></ng-content>',
    styles:[':host(tb-dynamic-cmp-tree){display:flex;flex:1;overflow:hidden;}'],
    providers: [EventDataService, Store]
})
export class DynamicCmpComponentTree {

}
@Component({
    selector: 'tb-dynamic-cmp',
    styles: [':host(tb-dynamic-cmp){flex:1;overflow:hidden;}'],
    template: '<div #cmpContainer></div><tb-message-dialog></tb-message-dialog><tb-diagnostic-dialog></tb-diagnostic-dialog><tb-dynamic-dialog></tb-dynamic-dialog>'
})
export class DynamicCmpComponent implements OnInit, OnDestroy {
    cmpRef: ComponentRef<DocumentComponent>;
    @Input() componentInfo: ComponentInfo;
    @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;
    @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;
    @ViewChild(DiagnosticDialogComponent) diagnosticDialog: DiagnosticDialogComponent;
    @ViewChild(DynamicDialogComponent) dynamicDialog: DynamicDialogComponent;
    subscriptions = [];

    constructor(public componentService: ComponentService) {
    }

    ngOnInit() {
        this.createComponent();
    }
    createComponent() {
        if (this.componentInfo) {
            this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
            this.cmpRef.instance.cmpId = this.componentInfo.id; //assegno l'id al componente
            if (this.cmpRef.instance.ciService)
                this.cmpRef.instance.ciService.componentInfo = this.componentInfo;
            //per i componenti slave, documento ed eventi sono condivisi col componente master
            if (!this.cmpRef.instance.document) {
                this.cmpRef.instance.document = this.componentInfo.document;
                this.cmpRef.instance.tbComponentService = this.componentInfo.document;
            }
            else {
                this.cmpRef.instance.document.init(this.componentInfo.id); //assegno l'id al servizio (uguale a quello del componente)
            }
            this.cmpRef.instance.args = this.componentInfo.args;
            this.subscriptions.push(this.cmpRef.instance.document.eventData.openMessageDialog.subscribe(
                args => this.openMessageDialog(args)
            ));
            this.subscriptions.push(this.cmpRef.instance.document.eventData.openDiagnosticDialog.subscribe(
                data => this.openDiagnosticDialog(data)
            ));
            this.subscriptions.push(this.cmpRef.instance.document.eventData.openDynamicDialog.subscribe(
                data => this.openDynamicDialog(data)
            ));
            //se la eseguo subito, lancia un'eccezione quando esegue l'aggiornamento dei binding, come se fosse in un momento sbagliato
            setTimeout(() => {
                this.componentInfo.instance = this.cmpRef.instance;
                this.componentInfo.document = this.cmpRef.instance.document;
                this.componentService.onComponentCreated(this.componentInfo);

            }, 1);

        }
    }

    ngOnDestroy() {
        if (this.cmpRef) {
            this.cmpRef.destroy();
        }
        this.subscriptions.forEach(subs => subs.unsubscribe());
    }

    public openMessageDialog(args: MessageDlgArgs) {
        this.messageDialog.open(args, this.cmpRef.instance.document.eventData);
    }
    public openDiagnosticDialog(data: DiagnosticData) {
        this.diagnosticDialog.open(data, this.cmpRef.instance.document.eventData);
    }
    public openDynamicDialog(componentInfo: ComponentInfo) {
        componentInfo.document = this.cmpRef.instance.document;
        this.dynamicDialog.open(componentInfo);
    }
}