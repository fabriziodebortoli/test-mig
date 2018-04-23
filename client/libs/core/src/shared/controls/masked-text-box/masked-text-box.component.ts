import { Component, Input  } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-masked-text-box',
  templateUrl: './masked-text-box.component.html',
  styleUrls: ['./masked-text-box.component.scss']
})
export class MaskedTextBoxComponent extends ControlComponent  {
 @Input() forCmpID: string;
 @Input() disabled: boolean;
 @Input() mask: string;
 @Input() rules: { [key: string]: RegExp };
}
