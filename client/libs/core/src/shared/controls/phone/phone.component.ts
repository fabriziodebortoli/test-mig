import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-phone',
  templateUrl: './phone.component.html',
  styleUrls: ['./phone.component.scss']
})
export class PhoneComponent extends ControlComponent {
  @Input() public mask: string;
  //TODOLUCA, aggiungere derivazione da textedit, e spostare rows e chars come gestione nel componente text
  @Input('rows') rows: number = 0;
  @Input('chars') chars: number = 0;
}
