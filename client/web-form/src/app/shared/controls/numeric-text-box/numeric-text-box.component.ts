import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';


@Component({
  selector: 'tb-numeric-text-box',
  templateUrl: './numeric-text-box.component.html',
  styleUrls: ['./numeric-text-box.component.scss']
})
export class NumericTextBoxComponent extends ControlComponent {
@Input() forCmpID: string;
@Input() format: string;
}
