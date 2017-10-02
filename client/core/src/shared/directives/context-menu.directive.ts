import { Directive, ViewChild, ElementRef, AfterContentInit, ViewContainerRef, ComponentFactoryResolver, AfterViewInit, ComponentRef, Input } from '@angular/core';

import { ContextMenuComponent } from './../components/context-menu/context-menu.component';

@Directive({
  selector: '[tbContextMenu]',
})
export class ContextMenuDirective implements AfterContentInit {

  @Input() tbContextMenu: any;

  @ViewChild('contextMenu', { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  public contextMenuRef: ComponentRef<any>;

  public cm: ViewContainerRef;

  constructor(
    public vcr: ViewContainerRef,
    public componentResolver: ComponentFactoryResolver) { }

  renderComponent() {
    // if (this.contextMenuRef) this.contextMenuRef.instance.value = this.value;
  }

  ngAfterContentInit() {

    this.cm = (<any>this.vcr)._data.componentView.component.contextMenu;
    let componentFactory = this.componentResolver.resolveComponentFactory(ContextMenuComponent);
    this.contextMenuRef = this.cm.createComponent(componentFactory);

    this.contextMenuRef.instance.contextMenuBinding = this.tbContextMenu;
    this.renderComponent();
  }

  ngOnChanges(changes: Object) {
    this.renderComponent();
  }
}
