import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input, OnInit, OnChanges, AfterViewInit } from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';


@Component({
  selector: 'tb-email',
  templateUrl: './email.component.html',
  styleUrls: ['./email.component.scss']
})

export class EmailComponent extends ControlComponent implements OnInit, OnChanges, AfterViewInit {
  @Input('readonly') readonly: boolean = false;
  errorMessage: string;
  showError = '';
  mask = '';
  public constraint: RegExp;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService) {
    super(layoutService, tbComponentService);

  }

  ngOnInit() {
  }

  public onChange(val: any) {
    this.onUpdateNgModel(val);
  }

  onUpdateNgModel(newValue: string): void {
    if (!this.modelValid()) {
      this.model = { enable: 'true', value: '' };
    }
    this.model.value = newValue;
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
  modelValid() {
    return this.model !== undefined && this.model !== null;
  }


  onBlur(): any {
    this.constraint = new RegExp('^[a-zA-Z0-9_\+-]+(\.[a-zA-Z0-9_\+-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.([a-zA-Z]{2,})$', 'i');
    if (!this.constraint.test(this.model.value)) {
      this.errorMessage = 'Input not in correct form';
      this.showError = 'inputError';
    }
    else {
      this.errorMessage = '';
      this.showError = '';
    }

    this.eventData.change.emit(this.cmpId);
  }
}
