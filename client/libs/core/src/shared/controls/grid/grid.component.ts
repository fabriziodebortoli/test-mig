import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { Subscription } from '../../../rxjs.imports';

import { DataService } from './../../../core/services/data.service';

@Component({
  selector: 'tb-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.scss']
})
export class GridComponent implements OnInit, OnDestroy {

  @Input() gridNamespace: string;
  @Input() gridSelectionType: string;
  @Input() gridParams: URLSearchParams;

  public dataSubscription: Subscription;
  public gridColumns: string[];
  public gridData: any[];



  constructor(public dataService: DataService) { }

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
