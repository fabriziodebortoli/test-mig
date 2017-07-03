import { Component, OnChanges, AfterViewInit, Input } from '@angular/core';
import { ControlComponent } from './../control.component';
import { EventDataService } from '@taskbuilder/core';

@Component({
  selector: 'tb-time-input',
  templateUrl: './time-input.component.html',
  styleUrls: ['./time-input.component.scss']
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
    this.model.value = 60 * ( 60 * r.getHours() + r.getMinutes() ) + r.getSeconds();
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
