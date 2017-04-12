import { Component, Input, ViewChild, OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
// import * as moment from 'moment';
import { formatDate } from '@telerik/kendo-intl';
import { EventDataService } from './../../../core/eventdata.service';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})

export class DateInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @Input() formatter: string;
  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'left', vertical: 'top' };

  selectedDate: Date;
  popupOpen = false;
  doubleEvent = false;
  dateFormat = 'dd MMM yyyy';

  constructor(private eventData: EventDataService) {
    super();
  }

  public handleChange(value: Date): void {
    this.onUpdateNgModel(value);
    this.onClickIconCalend();
  }

  public onChange(val: any) {
    this.onUpdateNgModel(val);
  }

  onClickIconCalend(): void {
    this.popupOpen = !this.popupOpen;
    this.doubleEvent = false;
  }

  onBlurCalendar(): void {
    this.popupOpen = this.doubleEvent;
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }

  private press(): void { // necessario per evitare che sul click di chiusura, il blur annulli il click
    this.doubleEvent = true;
  }

  onUpdateNgModel(newDate: Date): void {
    let timestamp = Date.parse(newDate.toDateString() )
    if (isNaN(timestamp)) {  return;  }
    if (this.model === null) {
      this.model = { enable: 'true', value: '' };
    }

    this.selectedDate = newDate;
    this.model.value = formatDate(this.selectedDate, 'y-MM-ddTHH:mm:ss');
    console.log('this.model.value = ' + this.model.value);
  }

  ngAfterViewInit(): void {
    if (this.model !== undefined && this.model !== null) {
      this.onUpdateNgModel(new Date(this.model.value));
    }
  }

  ngOnChanges(): void {
    if (this.model !== undefined && this.model !== null) {
      this.onUpdateNgModel(new Date(this.model.value));
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

}
