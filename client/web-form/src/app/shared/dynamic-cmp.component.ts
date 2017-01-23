import { DocumentComponent } from '.';
import { ComponentInfo } from './models/component.info';
import { Component, ViewContainerRef, OnInit, OnDestroy, ComponentRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-dynamic-cmp',
  template: '<div #cmpContainer></div>'
})
export class DynamicCmpComponent implements OnInit, OnDestroy {
  cmpRef: ComponentRef<DocumentComponent>;

  title: string = "";
  @Input() componentInfo: ComponentInfo;
  @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;

  constructor() {
  }

  ngOnInit() {
    if (this.componentInfo) {
      this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
      this.cmpRef.instance.cmpId = this.componentInfo.id;//assegno l'id al componente
      this.cmpRef.instance.document.mainCmpId = this.componentInfo.id;//assegno l'id al servizio (uguale a quello del componente)
      this.title = this.cmpRef.instance.title;
    }
  }

  ngOnDestroy() {
    if (this.cmpRef) {
      this.cmpRef.destroy();
    }

  }
}