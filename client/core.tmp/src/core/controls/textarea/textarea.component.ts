import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-textarea',
  templateUrl: './textarea.component.html',
  styleUrls: ['./textarea.component.scss']
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
