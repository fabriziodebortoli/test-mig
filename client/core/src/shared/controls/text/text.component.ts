import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit, Output, EventEmitter } from '@angular/core';

// import { ContextMenuComponent } from './../context-menu/context-menu.component';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from '../control.component';

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent /*implements AfterContentInit, OnChanges */ {

  @Input('readonly') readonly: boolean = false;
  @Input() public hotLink: any = undefined;

  @ViewChild("contextMenu", { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  // private contextMenuRef;

  constructor(
    private eventData: EventDataService,
    private vcr: ViewContainerRef,
    private componentResolver: ComponentFactoryResolver,
    layoutService: LayoutService,
    tbComponentService: TbComponentService
  ) {
    super(layoutService, tbComponentService);
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
