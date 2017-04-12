import { Component, Input} from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-phone',
  templateUrl: './phone.component.html',
  styleUrls: ['./phone.component.scss']
})
export class PhoneComponent extends ControlComponent {
  @Input() public mask: string = "(999) 0000000000";
}
