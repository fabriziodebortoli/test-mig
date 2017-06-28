import { Component, Input, Output, EventEmitter } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-combo-simple',
  templateUrl: './combo-simple.component.html',
  styleUrls: ['./combo-simple.component.scss']
})
export class ComboSimpleComponent extends ControlComponent {

  @Input() public items: Array<any> = [];
  @Input() public defaultItem: any;
  @Output('changed') changed: EventEmitter<any> = new EventEmitter();

  public selectionChange(value: any): void {
    this.model.value = value.code;
    this.changed.emit(this);
  }
}
