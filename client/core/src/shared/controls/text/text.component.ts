import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import {
  Component, Input, ViewChild, ViewContainerRef, ComponentFactoryResolver, ComponentRef,
  OnChanges, AfterContentInit, OnInit, Output, HostListener, EventEmitter, ChangeDetectorRef, SimpleChanges
} from '@angular/core';
import { Store } from './../../../core/services/store.service';

import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from '../control.component';
import { Subscription } from '../../../rxjs.imports';

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent implements OnChanges, OnInit {

  @Input('readonly') readonly: boolean = false;
  @Input() public hotLink: { namespace: string, name: string };
  @Input('rows') rows: number = 0;
  @Input('chars') chars: number = 0;
  @Input('textLimit') textlimit: number = 0;
  @Input('maxLength') maxLength: number = 524288;
  @Input('multiline') multiline: boolean = false;

  //public mask = '';
  //public maxLenght = 0;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    private store: Store
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.store.select(_ => this.model && this.model.length).
      subscribe(l => this.onlenghtChanged(l));
  }

  onlenghtChanged(len: any) {
    if (len !== undefined)
      this.setlength(len);
  }

  onBlur($event) {
    if ($event == undefined)
      return;

    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  // Metodo con OnChanges
  ngOnChanges(changes: SimpleChanges) {
    if (!changes.model || !this.model || !this.model.length)
      return;

    this.setlength(this.model.length)
  }

  setlength(len: number) {
    this.maxLength = this.model ? this.model.length : 0;
    if (this.textlimit > 0 && (this.maxLength == 0 || this.textlimit < this.maxLength)) {
      this.maxLength = this.textlimit;
    }
  }
}
