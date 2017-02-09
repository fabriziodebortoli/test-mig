import { Component, OnInit, Input } from '@angular/core';
import { ReportObject, ReportObjectType } from '../report.model';

@Component({
  selector: 'rs-report-object',
  templateUrl: './report-object-wrapper.component.html',
  styleUrls: ['./report-object-wrapper.component.css']
})
export class ReportObjectWrapperComponent implements OnInit {

  public reportObjectType = ReportObjectType;

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log(this.ro);
  }

}
