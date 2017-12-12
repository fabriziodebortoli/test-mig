import { Component, Input, ViewChild, OnChanges, OnInit, AfterViewInit, SimpleChanges, ChangeDetectorRef, Inject } from '@angular/core';

import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { formatDate } from '@telerik/kendo-intl';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { FormattersService } from './../../../core/services/formatters.service';

import { ControlComponent } from './../control.component';

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

  formatter: any;
  selectedDate: Date;
  dateFormat = 'dd MMM yyyy';

  nullDate = new Date("1799-12-31 00:00:00");

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

    if (!this.format)
      this.format = "Date";
    this.formatter = this.formattersService.getFormatter(this.format);
  }

  dateFormatByFormatter() {
    let ret = '', day = '', month = '', year = '';
    let dateSep1 = this.formatter.refFirstSeparator ? this.formatter.refFirstSeparator : " ";
    let dateSep2 = this.formatter.refSecondSeparator ? this.formatter.refSecondSeparator : " ";
    let timeSep = this.formatter.refTimeSeparator ? this.formatter.refTimeSeparator : ":";

    day = String('d').repeat((<string>this.formatter.DayFormat).length - 3);    // 'DAY99'
    month = String('M').repeat((<string>this.formatter.MonthFormat).length - 5);  // 'MONTH99'
    year = String('y').repeat((<string>this.formatter.YearFormat).length - 4);   // 'YEAR99'


    switch (this.formatter.FormatType) {
      case "DATE_DMY": {
        ret = day + dateSep1 + month + dateSep2 + year;
        break;
      }
      case "DATE_YMD": {
        ret = year + dateSep1 + month + dateSep2 + day;
        break;
      }
      default: {
        ret = month + dateSep1 + day + dateSep2 + year;   // "DATE_MDY"
        break;
      }
    }

    if (this.format === "DateTime") {
      ret = ret + " HH" + timeSep + "mm";
    }

    if (this.format === "DateTimeExtended") {
      ret = ret + " HH" + timeSep + "mm" + timeSep + "ss";
    }

    return ret;
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

  getFormat(): string {
    if (this.formatter) {
      this.dateFormat = this.dateFormatByFormatter();
    }
    else {
      switch (this.format) {
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
