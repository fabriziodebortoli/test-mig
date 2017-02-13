import { DocumentComponent } from '.';
import { ComponentInfo } from './models/component.info';
import { Component, ViewContainerRef, OnInit, OnDestroy, ComponentRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-dynamic-cmp',
  template: '<div #cmpContainer></div>'
})
export class DynamicCmpComponent implements OnInit, OnDestroy {
  cmpRef: ComponentRef<DocumentComponent>;
  @Input() componentInfo: ComponentInfo;
  @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;

  constructor() {
  }

  ngOnInit() {
    if (this.componentInfo) {
      this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
      this.cmpRef.instance.cmpId = this.componentInfo.id;//assegno l'id al componente

      if (this.cmpRef.instance.document) {
        this.cmpRef.instance.document.init(this.componentInfo.id);//assegno l'id al servizio (uguale a quello del componente)
      }

      //se la eseguo subito, lancia un'eccezione quando esegue l'aggiornamento dei binding, come se fosse in un momento sbagliato
      setTimeout(() => { this.componentInfo.document = this.cmpRef.instance.document; }, 1);
    }
  }

  ngOnDestroy() {
    if (this.cmpRef) {
      this.cmpRef.destroy();
    }

  }
}