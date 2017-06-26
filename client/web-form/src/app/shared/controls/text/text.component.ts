import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit, Output, EventEmitter } from '@angular/core';
// import { ContextMenuComponent } from './../context-menu/context-menu.component';
import { EventDataService } from '@taskbuilder/core';
import { ControlComponent } from '../control.component';

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent /*implements AfterContentInit, OnChanges */ {

  @Input('readonly') readonly: boolean = false;
  @Input() public hotLink: any = undefined;

  @Input() width: number;

  @ViewChild("contextMenu", { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  // private contextMenuRef;

  constructor(private eventData: EventDataService, private vcr: ViewContainerRef, private componentResolver: ComponentFactoryResolver) {
    super();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  ngAfterContentInit() {

    // let componentFactory = this.componentResolver.resolveComponentFactory(ContextMenuComponent);
    // this.contextMenuRef = this.contextMenu.createComponent(componentFactory);
  }

  // ngOnChanges(changes: Object) {
  //   console.log("ngOnChanges", changes);
  // }
}
