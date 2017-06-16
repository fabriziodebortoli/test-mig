import { Component, Input, OnInit, OnChanges, AfterViewInit } from '@angular/core';

import { ControlComponent } from './../control.component';

import { EventDataService } from './../../services/eventdata.service';

@Component({
  selector: 'tb-email',
  template: "<div class=\"tb-control tb-email\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <!--<input id=\"{{cmpId}}\" type=\"text\" [ngClass]=\"showError\" class=\"tb-text\" (blur)=\"onBlur()\" [disabled]=\"!model?.enabled\" [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [readonly]=\"readonly\" />--> <kendo-maskedtextbox id=\"{{cmpId}}\"  [mask]=\"mask\" [ngClass]=\"showError\" class=\"tb-text\" [ngModel]=\"model?.value\" (blur)=\"onBlur()\" [disabled]=\"!(model?.enabled)\" (ngModelChange)=\"model.value=$event\"  ></kendo-maskedtextbox> <div class=\"has-error\">{{errorMessage}}</div> </div>",
  styles: [""]
})
export class EmailComponent extends ControlComponent implements OnInit, OnChanges, AfterViewInit {
  @Input('readonly') readonly: boolean = false;
  private errorMessage: string;
  private showError = '';
  private constraint: RegExp;

  constructor(private eventData: EventDataService) {
    super();

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
