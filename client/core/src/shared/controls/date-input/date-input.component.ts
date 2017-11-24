import { Component, Input, ViewChild, OnChanges, AfterViewInit, SimpleChanges, ChangeDetectorRef } from '@angular/core';

import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { formatDate } from '@telerik/kendo-intl';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})
export class DateInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @Input() formatter: string;
  @Input() readonly = false;

  selectedDate: Date;
  dateFormat = 'dd MMM yyyy';

  nullDate = new Date("1799-12-31 00:00:00");

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  onChange(changes) {
    this.onUpdateNgModel(changes);
  }

  onBlur(changes: SimpleChanges) {
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  onUpdateNgModel(newDate: Date, updateModel: boolean = true): void {
    // if (!newDate) {
    //   return;
    // }
    // const timestamp = Date.parse(newDate.toDateString())
    // if (isNaN(timestamp)) { return; }
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }

    if (!newDate || newDate.getTime() === this.nullDate.getTime()) {
      this.selectedDate = null;
    }
    else {
      this.selectedDate = newDate;
    }


    //if (this.model.constructor.name === 'text') {
    if (updateModel)
      this.model.value = formatDate(this.selectedDate ? this.selectedDate : this.nullDate, 'y-MM-ddTHH:mm:ss');
    //}
  }

  ngAfterViewInit(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(new Date(this.model.value), false);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(new Date(this.model.value), false);
    }
  }

  getFormat(formatter: string): string {
    switch (formatter) {
      case 'Date':
        this.dateFormat = 'dd MMM yyyy'; break;
      case 'DateTime':
        this.dateFormat = 'dd MMM yyyy HH:mm'; break;
      case 'DateTimeExtended':
        this.dateFormat = 'dd MMM yyyy HH:mm:ss'; break;
      default: break;
    }
    return this.dateFormat;
  }

  modelValid() {
    return this.model !== undefined && this.model !== null;
  }

}
