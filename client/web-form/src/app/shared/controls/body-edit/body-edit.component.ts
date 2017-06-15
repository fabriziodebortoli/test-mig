import { ControlComponent } from './../control.component';
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
/*scommentare quanto ci sarà il binding on demand
 import { Observable } from 'rxjs/Rx';
 import { State } from '@progress/kendo-data-query';
 import {
     GridDataResult,
     DataStateChangeEvent
 } from '@progress/kendo-angular-grid';
 */


@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss']
})
export class BodyEditComponent extends ControlComponent {
  @Input() columns: Array<any>;
  private gridData: Array<any>;
/*  private gridView: GridDataResult;*/

/* SORTING
private sort: SortDescriptor[] = [];
    private gridView: GridDataResult;
 */
  constructor() {
    super();
  }

  /* SORTING
  protected sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        this.loadProducts();
    }
    */

    /* BINDING
    private loadProducts(): void {
      this.gridData = this.gridData.slice(this.skip, this.skip + this.pageSize),
          this.gridView = {
            data: orderBy(this.gridData, this.sort),
            total: this.gridData.length
        };
    }
     */
    /* PAGING
    protected pageChange({ skip, take }: PageChangeEvent): void {
        this.skip = skip;
        this.pageSize = take;
        this.loadProducts();
    }
    */
}
