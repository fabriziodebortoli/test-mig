import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-body-edit',
  template: "<kendo-grid [kendoGridBinding]=\"model\"> <kendo-grid-column *ngFor=\"let column of columns\" field=\"{{column?.binding?.datasource}}\" title=\"{{column?.text}}\" width=\"200\"> <template kendoGridCellTemplate let-dataItem> <div>{{dataItem[column.binding.datasource]?.value}}</div> </template> </kendo-grid-column> </kendo-grid>",
  styles: [""]
})
export class BodyEditComponent extends ControlComponent {

  @Input() columns: Array<any>;

  constructor() {
    super();
  }

}
