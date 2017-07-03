import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, OnChanges, AfterContentInit, Output, EventEmitter } from '@angular/core';

import { ControlComponent } from '../control.component';

import { EventDataService } from './../../services/eventdata.service';

@Component({
  selector: 'tb-text',
  template: "<div class=\"tb-control tb-text\"> <tb-caption caption=\"{{caption}}\" forCmpID=\"forCmpID\"></tb-caption> <div class=\"group\"> <kendo-maskedtextbox [mask]=\"mask\" [ngModel]=\"model?.value\" required=\"required\" (blur)=\"onBlur()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"model.value=$event\" [style.width.px]=\"width\"></kendo-maskedtextbox> <ng-container #contextMenu></ng-container> </div> </div>",
  styles: [""]
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
