import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-textarea',
  template: "<div class=\"tb-control tb-textarea\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <textarea name=\"{{cmpId}}\" [readonly]=\"readonly\" [ngModel]=\"model?.value\"  (ngModelChange)=\"model.value=$event\" [disabled]=\"!model?.enabled\" [ngStyle]=\"{'height': getCorrectHeight(), 'width':  getCorrectWidth()}\"></textarea> <ng-container #contextMenu></ng-container> </div>",
  styles: ["textarea { width: 100%; background: #ffffde; border: 1px solid #ddd; } textarea[disabled] { width: 100%; background-color: #f3f3f3; border: 1px solid #ddd; font-weight: bold; color: #9c9c9c; font-family: inherit; } "]
})
export class TextareaComponent extends ControlComponent {
  @Input('readonly') readonly = false;
  @Input() width: number;
  @Input() height: number;
  constructor() {
    super();
  }

  getCorrectHeight() {
    return isNaN(this.height) ? this.height.toString() : this.height + 'px';;
  }


  getCorrectWidth() {
    return isNaN(this.width) ? this.width.toString() : this.width + 'px';
  }
}
