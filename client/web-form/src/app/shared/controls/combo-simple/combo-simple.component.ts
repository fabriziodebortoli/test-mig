import { ControlComponent } from './../control.component';
import { Component, OnInit, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'tb-combo-simple',
  templateUrl: './combo-simple.component.html',
  styleUrls: ['./combo-simple.component.scss']
})
export class ComboSimpleComponent extends ControlComponent {

  @Input() public items: Array<any> = [];
  @Input() public defaultItem: any;

 public selectionChange(value: any): void {
        this.model.value = value.code;
    }
}
