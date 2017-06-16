import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { Subscription } from 'rxjs';

import { DataService } from './../../services/data.service';

@Component({
  selector: 'tb-grid',
  template: "<kendo-grid [data]=\"gridData\" [height]=\"370\"> <kendo-grid-column *ngFor=\"let col of gridColumns\" field=\"{{col}}\" title=\"{{col}}\"></kendo-grid-column> </kendo-grid>",
  styles: [""]
})
export class GridComponent implements OnInit, OnDestroy {

  @Input() gridNamespace: string;
  @Input() gridSelectionType: string;
  @Input() gridParams: URLSearchParams;

  private dataSubscription: Subscription;
  private gridColumns: string[];
  private gridData: any[];

  constructor(private dataService: DataService) { }

  ngOnInit() {
    // this.dataService.getColumns(this.gridNamespace, this.gridSelectionType).subscribe(columns => this.gridColumns = columns);

    this.dataSubscription = this.dataService.getData(this.gridNamespace, this.gridSelectionType, this.gridParams).subscribe((data: any) => {
      this.gridColumns = data.titles;
      this.gridData = data.rows;
    });
  }

  ngOnDestroy() {
    this.dataSubscription.unsubscribe();
  }

}
