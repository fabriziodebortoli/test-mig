import { ComponentInfo } from './models/component.info';
import { TbComponent } from './tb.component';
import { Component, ViewContainerRef, OnInit, OnDestroy, ComponentRef, Input, ViewChild } from '@angular/core';

@Component({
  selector: 'tb-dynamic-cmp',
  template: '<div #cmpContainer></div>'
})
export class DynamicCmpComponent implements OnInit, OnDestroy {
  cmpRef: ComponentRef<TbComponent>;
  @Input() componentInfo: ComponentInfo;
  @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;

  constructor() {
  }

  ngOnInit() {
    if (this.componentInfo) {
      this.cmpRef = this.cmpContainer.createComponent(this.componentInfo.factory);
      this.cmpRef.instance.cmpId = this.componentInfo.id;//assegni l'id al componente
    }
  }

  ngOnDestroy() {
    if (this.cmpRef) {
      this.cmpRef.destroy();
    }

  }
}