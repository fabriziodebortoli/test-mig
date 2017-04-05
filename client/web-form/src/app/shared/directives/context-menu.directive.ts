import { ContextMenuComponent } from './../controls/context-menu/context-menu.component';
import { Directive, ViewChild, ElementRef, AfterContentInit, ViewContainerRef, ComponentFactoryResolver, AfterViewInit, ComponentRef } from '@angular/core';

@Directive({
  selector: '[tbContextMenu]'
})
export class ContextMenuDirective implements AfterContentInit {

  @ViewChild('contextMenu', { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  private contextMenuRef: ComponentRef<any>;

  private cm: ViewContainerRef;

  constructor(private vcr: ViewContainerRef, private componentResolver: ComponentFactoryResolver) {
    console.log(vcr);
  }

  renderComponent() {
    // if (this.contextMenuRef) this.contextMenuRef.instance.value = this.value;
  }

  ngAfterContentInit() {

    this.cm = (<any>this.vcr)._data.componentView.component.contextMenu;
    console.log("_data.componentView", this.cm);

    let componentFactory = this.componentResolver.resolveComponentFactory(ContextMenuComponent);
    this.contextMenuRef = this.cm.createComponent(componentFactory);
    this.renderComponent();
  }

  ngOnChanges(changes: Object) {
    this.renderComponent();
  }
}
