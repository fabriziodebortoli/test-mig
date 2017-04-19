import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-textarea',
  templateUrl: './textarea.component.html',
  styleUrls: ['./textarea.component.scss']
})
export class TextareaComponent extends ControlComponent {
@Input('readonly') readonly = false;

  constructor() {
  super();

  }

  ngOnInit() {
  }

}
