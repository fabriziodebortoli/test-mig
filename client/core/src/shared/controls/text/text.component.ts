import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Store } from './../../../core/services/store.service';
import {
  Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef,
  OnChanges, AfterContentInit, OnInit, Output, EventEmitter, ChangeDetectorRef, SimpleChanges
} from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from '../control.component';
import { Subscription } from '../../../rxjs.imports';

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent implements OnChanges {

  @Input('readonly') readonly: boolean = false;
  @Input() public hotLink: { namespace: string, name: string };
  @Input('rows') rows: number = 0;
  @Input('textlimit') textlimit: number = 0;
  @Input('maxLength') maxLength: number = 0;
  @Input('multiline') multiline: boolean = false;

  //public mask = '';
  //public maxLenght = 0;

  constructor(
    public eventData: EventDataService,
    public vcr: ViewContainerRef,
    public componentResolver: ComponentFactoryResolver,
    private store: Store,
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

  // Metodo con OnChanges
  ngOnChanges(changes: SimpleChanges) {
    if (!changes.model ||   !this.model || !this.model.length)
      return;

    this.setlength(this.model.length)
  }

  setlength(len: number) {
    this.maxLength = this.model ? this.model.length : 0;
    if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
      this.maxLength = this.textlimit;
    }

    // Metodo con Store
    // ngOnInit() {
    //   this.store
    //     .select(_ => this.model && this.model.length)
    //     .subscribe(this.handleLength);
    // }

    // handleLength = length => {
    //   this.maxLength = this.model ? this.model.length : 0;
    //   if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
    //     this.maxLength = this.textlimit;
    //   }
  }
}
