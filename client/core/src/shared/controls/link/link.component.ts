import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, OnChanges, AfterViewInit, ChangeDetectorRef } from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-link',
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss']
})
export class LinkComponent extends ControlComponent implements OnInit, OnChanges, AfterViewInit {

  public selectedValue: string;
  @Input() pattern: string;
  public constraint: RegExp;

  errorMessage: string;
  showError = '';
  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);

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
    this.selectedValue = newValue;
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
    this.constraint = new RegExp('((http|https)(:\/\/))?([a-zA-Z0-9]+[.]{1}){2}[a-zA-z0-9]+(\/{1}[a-zA-Z0-9]+)*\/?', 'i');
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
