import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-phone',
  template: "<div class=\"tb-control tb-phone\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-maskedtextbox [mask]=\"mask\" [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [disabled]=\"!model?.enabled\"></kendo-maskedtextbox> </div>",
  styles: [""]
})
export class PhoneComponent extends ControlComponent {
  @Input() public mask: string;
}
