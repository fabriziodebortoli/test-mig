import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-password',
  templateUrl: './password.component.html',
  styleUrls: ['./password.component.scss']
})
export class PasswordComponent  extends ControlComponent {
@Input() forCmpID: string;
}
