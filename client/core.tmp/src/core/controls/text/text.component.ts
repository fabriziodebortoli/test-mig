import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit, Output, EventEmitter } from '@angular/core';

import { ControlComponent } from '../control.component';

import { EventDataService } from './../../services/eventdata.service';

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent /*implements AfterContentInit, OnChanges */ {

  @Input('readonly') readonly = false;
  @Input() public hotLink: any = undefined;

  @Input() width: number;

  @ViewChild('contextMenu', { read: ViewContainerRef }) contextMenu: ViewContainerRef;

  constructor(private eventData: EventDataService, private vcr: ViewContainerRef, private componentResolver: ComponentFactoryResolver) {
    super();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

}
