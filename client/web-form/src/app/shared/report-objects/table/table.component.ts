import { GridModule } from '@progress/kendo-angular-grid';
import { table } from './../../../reporting-studio/reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class ReportTableComponent implements OnInit {

  @Input() table: table;

  public data: any[] = [];

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectTableComponent', this.ro);
  }

}
