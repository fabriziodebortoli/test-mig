import { Component } from '@angular/core';

import { ControlComponent } from '../control.component';

@Component({
  selector: 'tb-color-picker',
  template: "<div class=\"tb-control tb-color-picker\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input id=\"{{cmpId}}\" type=\"text\" (blur)=\"onBlur()\" [disabled]=\"!model?.enabled\" [ngModel]=\"model?.value\" (ngModelChange)=\"model.value=$event\" [placeholder]=\"caption\" class=\"tb-color-picker\" /> </div>",
  styles: [""]
})
export class ColorPickerComponent extends ControlComponent {

  constructor() {
    super();
  }

}
