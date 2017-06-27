import { Component, Input, Output, EventEmitter } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-combo-simple',
  template: "<div class=\"tb-control tb-combo-simple\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <kendo-dropdownlist [data]=\"items\" [textField]=\"'description'\" [disabled]=\"!model?.enabled\" [valueField]=\"'code'\" [defaultItem]=\"defaultItem\" (selectionChange)=\"selectionChange($event)\"> <template kendoDropDownListNoDataTemplate> <h4><span class=\"k-icon k-i-warning\"></span><br /><br /> No data here</h4> </template> </kendo-dropdownlist> </div>",
  styles: [".k-i-warning { font-size: 2.5em; } h4 { font-size: 1em; } "]
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

