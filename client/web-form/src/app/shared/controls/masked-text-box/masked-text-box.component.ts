import { ControlComponent } from './../control.component';
import { Component, Input  } from '@angular/core';

@Component({
  selector: 'tb-masked-text-box',
  templateUrl: './masked-text-box.component.html',
  styleUrls: ['./masked-text-box.component.scss']
})
export class MaskedTextBoxComponent extends ControlComponent  {
 @Input() mask: string;
 @Input() forCmpID: string;
}
