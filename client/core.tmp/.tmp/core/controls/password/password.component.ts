import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-password',
  template: "<div class=\"tb-control tb-password\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input type=\"password\" [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\" /> </div>",
  styles: [""]
})
export class PasswordComponent extends ControlComponent {
  @Input() forCmpID: string;
}
