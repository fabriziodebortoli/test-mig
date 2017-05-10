import { MessageDialogComponent, MessageDlgArgs } from './containers/message-dialog/message-dialog.component';
import { ComponentService } from './../core/component.service';
import { EventDataService } from './../core/eventdata.service';
import { BOService } from './../core/bo.service';
import { DocumentComponent } from '.';
import { ComponentInfo } from './models/component.info';
import { Component, ViewContainerRef, OnInit, OnDestroy, ComponentRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-dynamic-cmp',
  template: '<div #cmpContainer></div><tb-message-dialog></tb-message-dialog>'
})
export class DynamicCmpComponent implements OnInit, OnDestroy {
  cmpRef: ComponentRef<DocumentComponent>;
  @Input() componentInfo: ComponentInfo;
  @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;
  @ViewChild(MessageDialogComponent) messageDialog: MessageDialogComponent;
  messageDialogOpenSubscription: any;

  constructor(private componentService: ComponentService) {
  }

  ngOnInit() {
    this.createComponent();
  }
  createComponent() {
    if (this.componentInfo) {
      this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
      this.cmpRef.instance.cmpId = this.componentInfo.id; //assegno l'id al componente

      this.cmpRef.instance.document.init(this.componentInfo.id); //assegno l'id al servizio (uguale a quello del componente)

      this.cmpRef.instance.args = this.componentInfo.args;
      this.messageDialogOpenSubscription = this.cmpRef.instance.document.eventData.openMessageDialog.subscribe(
        args => this.openMessageDialog(this.cmpRef.instance.cmpId, args)
      );

      //se la eseguo subito, lancia un'eccezione quando esegue l'aggiornamento dei binding, come se fosse in un momento sbagliato
      setTimeout(() => {
        this.componentInfo.document = this.cmpRef.instance.document;
        this.componentService.onComponentCreated(this.componentInfo);
      }, 1);

    }
  }

  ngOnDestroy() {
    if (this.cmpRef) {
      this.cmpRef.destroy();
    }
    if (this.messageDialogOpenSubscription) {
      this.messageDialogOpenSubscription.unsubscribe();
    }
  }

  public openMessageDialog(mainCmpId: string, args: MessageDlgArgs) {
    this.messageDialog.open(args, this.cmpRef.instance.document.eventData);
  }
}