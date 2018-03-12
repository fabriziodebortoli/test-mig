import { Component, Input, ViewChild, OnChanges, OnInit, AfterViewInit, SimpleChanges, ChangeDetectorRef, Inject } from '@angular/core';

import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { formatDate } from '@telerik/kendo-intl';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { FormattersService } from './../../../core/services/formatters.service';

import { ControlComponent } from './../control.component';
import { getDateFormatByFormatter, NullDate } from './u';

// failed test to refresh locale settings immediately after logout-login

// import { LOCALE_ID } from '@angular/core';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})
export class DateInputComponent extends ControlComponent implements OnInit, OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @Input() readonly = false;

  formatterProps: any;
  selectedDate: Date;
  dateFormat = 'dd MMM yyyy';

  constructor(
    public eventData: EventDataService,
    private formattersService: FormattersService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);


    // failed test to refresh locale settings immediately after logout-login

    // @Inject(LOCALE_ID) private _locale: string
    // this._locale = localStorage.getItem('ui_culture');
  }

  ngOnInit() {

    if (!this.formatter)
      this.formatter = "Date";

    this.formatterProps = this.formattersService.getFormatter(this.formatter);
  }

  dateFormatByFormatter() {
    return getDateFormatByFormatter(this.formatterProps, this.formatter);
  }

  onChange(changes) {
    this.onUpdateNgModel(changes);
  }

  onBlur(changes: SimpleChanges) {
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  onUpdateNgModel(newDate: Date, updateModel: boolean = true): void {
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }

    if (!newDate || newDate.getTime() === NullDate.getTime()) {
      this.selectedDate = null;
    }
    else {
      this.selectedDate = newDate;
    }

    //if (this.model.constructor.name === 'text') {
    if (updateModel)
      this.model.value = formatDate(this.selectedDate ? this.selectedDate : NullDate, 'y-MM-ddTHH:mm:ss');
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

  getFormat(): string {
    if (this.formatter) {
      this.dateFormat = this.dateFormatByFormatter();
    }
    else {
      switch (this.formatter) {
        case 'Time':
        case 'ElapsedTime':
          this.dateFormat = 'HH:mm'; break;
        case 'Date':
          this.dateFormat = 'dd MMM yyyy'; break;
        case 'DateTime':
          this.dateFormat = 'dd MMM yyyy HH:mm'; break;
        case 'DateTimeExtended':
          this.dateFormat = 'dd MMM yyyy HH:mm:ss'; break;
        default: break;
      }
    }

    return this.dateFormat;
  }

  modelValid() {
    return this.model !== undefined && this.model !== null;
  }

}
