import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { Component, Input, ViewChild, OnChanges, AfterViewInit } from '@angular/core';
import { ControlComponent } from './../control.component';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
// import * as moment from 'moment';
// import { formatDate } from '@telerik/kendo-intl';

@Component({
  selector: 'tb-date-input',
  templateUrl: './date-input.component.html',
  styleUrls: ['./date-input.component.scss']
})

export class DateInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
  @Input() forCmpID: string;
  @Input() formatter: string;
  @Input() readonly = false;
  @Input() isReport = false;

  selectedDate: Date;
  dateFormat = 'dd MMM yyyy';

  constructor(
    private eventData: EventDataService,
    protected layoutService: LayoutService) {
    super(layoutService);
  }

  public onChange(val: any) {
    this.onUpdateNgModel(val);
  }


  onBlur() {
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }

  onUpdateNgModel(newDate: Date): void {
    if (!newDate) {
      return;
    }
    const timestamp = Date.parse(newDate.toDateString())
    if (isNaN(timestamp)) { return; }
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }

    this.selectedDate = newDate;
    // this.model.value = formatDate(this.selectedDate, 'y-MM-ddTHH:mm:ss');
  }

  ngAfterViewInit(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(new Date(this.model.value));
    }
  }

  ngOnChanges(): void {
    if (this.modelValid()) {
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

  modelValid() {
    return this.model !== undefined && this.model !== null;
  }

}
