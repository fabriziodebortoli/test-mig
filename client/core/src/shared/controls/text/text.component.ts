import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef, 
  OnChanges, AfterContentInit, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';

// import { ContextMenuComponent } from './../context-menu/context-menu.component';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from '../control.component';

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent implements AfterContentInit/*, OnChanges */ {

  @Input('readonly') readonly: boolean = false;
  @Input() public hotLink: { namespace: string, name: string};
  @Input('rows') rows: number = 0;
  @Input('textlimit') textlimit : number = 0;
  @Input('multiline') multiline : boolean = false;
  

  //@ViewChild("contextMenu", { read: ViewContainerRef }) contextMenu: ViewContainerRef;
  // public  contextMenuRef;

  public mask = '';
  public maxLenght = 0;

  constructor(
    public eventData: EventDataService,
    public vcr: ViewContainerRef,
    public componentResolver: ComponentFactoryResolver,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  onBlur($event) {
    if ($event == undefined)
      return;
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  ngAfterContentInit() {
    this.maxLenght = this.model ? this.model.length : 0;
    if (this.textlimit > 0 && (this.maxLenght == 0 || this.textlimit < this.maxLenght)) {
      this.maxLenght = this.textlimit;
    }
  }
}
