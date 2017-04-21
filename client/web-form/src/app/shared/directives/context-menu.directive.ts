import { ContextMenuComponent } from './../context-menu/context-menu.component';
import { Directive, ViewChild, ElementRef, AfterContentInit, ViewContainerRef, ComponentFactoryResolver, AfterViewInit, ComponentRef, Input } from '@angular/core';

@Directive({
  selector: '[tbContextMenu]',
})
export class ContextMenuDirective implements AfterContentInit {

  @Input() tbContextMenu: any;

  @ViewChild('contextMenu', { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  private contextMenuRef: ComponentRef<any>;

  private cm: ViewContainerRef;

  constructor(
    private vcr: ViewContainerRef,
    private componentResolver: ComponentFactoryResolver) { }

  renderComponent() {
    // if (this.contextMenuRef) this.contextMenuRef.instance.value = this.value;
  }

  ngAfterContentInit() {

    this.cm = (<any>this.vcr)._data.componentView.component.contextMenu;
    let componentFactory = this.componentResolver.resolveComponentFactory(ContextMenuComponent);
    this.contextMenuRef = this.cm.createComponent(componentFactory);

    console.log(this.tbContextMenu);
    this.contextMenuRef.instance.contextMenuBinding = this.tbContextMenu;
    this.renderComponent();
  }

  ngOnChanges(changes: Object) {
    this.renderComponent();
  }
}
