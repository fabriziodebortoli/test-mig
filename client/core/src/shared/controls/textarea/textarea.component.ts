import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-textarea',
  templateUrl: './textarea.component.html',
  styleUrls: ['./textarea.component.scss']
})
export class TextareaComponent extends ControlComponent {
  @Input('readonly') readonly = false;

  getCorrectHeight() {
   return isNaN(this.height) ?  this.height.toString() :  this.height +  'px';;
  }


  getCorrectWidth() {
     return isNaN(this.width) ? this.width.toString() : this.width + 'px';
  }
}
