import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit } from '@angular/core';
import { ContextMenuComponent } from './../context-menu/context-menu.component';
import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from "../control.component";

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent /*implements AfterContentInit, OnChanges */ {

  @ViewChild("contextMenu", { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  // private contextMenuRef;

  constructor(private eventData: EventDataService, private vcr: ViewContainerRef, private componentResolver: ComponentFactoryResolver) {
    super();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }

  ngAfterContentInit() {

    // let componentFactory = this.componentResolver.resolveComponentFactory(ContextMenuComponent);
    // this.contextMenuRef = this.contextMenu.createComponent(componentFactory);
  }

  // ngOnChanges(changes: Object) {
  //   console.log("ngOnChanges", changes);
  // }
}
