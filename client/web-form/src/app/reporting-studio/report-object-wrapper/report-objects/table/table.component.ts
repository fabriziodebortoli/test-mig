import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../../reporting-studio.model';

@Component({
  selector: 'rs-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class ReportObjectTableComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectTableComponent', this.ro);
  }

}
