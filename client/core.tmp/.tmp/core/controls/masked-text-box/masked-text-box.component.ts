import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-masked-text-box',
  template: "<div class=\"tb-control tb-masked-text-box\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-maskedtextbox [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [disabled]=\"!model?.enabled\" [mask]=\"mask\" [style.width.px]=\"width\"></kendo-maskedtextbox> </div>",
  styles: [""]
})
export class MaskedTextBoxComponent extends ControlComponent {
  @Input() forCmpID: string;
  @Input() disabled: boolean;
  @Input() mask: string;
  @Input() width: number;
}
