import { ComponentService } from './../core/component.service';
import { DocumentComponent } from '.';
import { ComponentInfo } from './models/component.info';
import { Component, ViewContainerRef, OnInit, OnDestroy, ComponentRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-dynamic-cmp',
  template: '<div #cmpContainer></div><div kendoDialogContainer></div>'
})
export class DynamicCmpComponent implements OnInit, OnDestroy {
  cmpRef: ComponentRef<DocumentComponent>;
  @Input() componentInfo: ComponentInfo;
  @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;

  constructor(private componentService: ComponentService) {
  }

  ngOnInit() {
    if (this.componentInfo) {
      this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
      this.cmpRef.instance.cmpId = this.componentInfo.id; //assegno l'id al componente

      this.cmpRef.instance.document.init(this.componentInfo.id); //assegno l'id al servizio (uguale a quello del componente)

      this.cmpRef.instance.args = this.componentInfo.args;
      //se la eseguo subito, lancia un'eccezione quando esegue l'aggiornamento dei binding, come se fosse in un momento sbagliato
      setTimeout(() => { this.componentInfo.document = this.cmpRef.instance.document; }, 1);

      setTimeout(() => this.componentService.onComponentCreated(this.componentInfo), 0);
    }
  }

  ngOnDestroy() {
    if (this.cmpRef) {
      this.cmpRef.destroy();
    }

  }
}