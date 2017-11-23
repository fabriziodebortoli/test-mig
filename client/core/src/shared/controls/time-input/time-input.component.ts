import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnChanges, AfterViewInit, Input, ChangeDetectorRef } from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-time-input',
  templateUrl: './time-input.component.html',
  styleUrls: ['./time-input.component.scss']
})
export class TimeInputComponent extends ControlComponent implements OnChanges, AfterViewInit {
  selectedTime: number = 0;
  @Input() forCmpID: string;
  @Input() formatter: string;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef:ChangeDetectorRef) {
    super(layoutService, tbComponentService,changeDetectorRef);
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
