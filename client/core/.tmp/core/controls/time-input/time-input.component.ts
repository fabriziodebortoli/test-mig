import { Component, OnChanges, AfterViewInit, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

import { EventDataService } from './../../services/eventdata.service';

@Component({
  selector: 'tb-time-input',
  template: "<div class=\"tb-control tb-time-input\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dateinput [(ngModel)]=\"selectedTime\" [format]=\"'HH:mm:ss'\" (valueChange)=\"onChange($event)\" (blur)=\"onBlur()\"></kendo-dateinput> </div>",
  styles: [""]
})
export class TimeInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
  selectedTime: number = 0;
  @Input() forCmpID: string;
  @Input() formatter: string;

  constructor(private eventData: EventDataService) {
    super();
  }

  public onChange(val: any) {
    this.onUpdateNgModel(val);
  }

  onUpdateNgModel(newTime: number): void {
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }
    this.selectedTime = newTime;
    let r = new Date(this.selectedTime);
    this.model.value = 60 * (60 * r.getHours() + r.getMinutes()) + r.getSeconds();
  }

  ngAfterViewInit(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(this.model.value);
    }
  }

  ngOnChanges(): void {
    if (this.modelValid()) {
      this.onUpdateNgModel(this.model.value);
    }
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }

  modelValid() {
    return this.model !== undefined && this.model !== null;
  }

}
