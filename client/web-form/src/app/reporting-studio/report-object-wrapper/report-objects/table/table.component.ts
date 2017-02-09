import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../../report.model';

@Component({
  selector: 'rs-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
export class ReportObjectTableComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectTableComponent', this.ro);
  }

}
