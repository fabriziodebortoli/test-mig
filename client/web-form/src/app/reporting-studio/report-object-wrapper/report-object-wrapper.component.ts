import { Component, OnInit, Input } from '@angular/core';
import { ReportObject, ReportObjectType } from '../reporting-studio.model';

@Component({
  selector: 'rs-report-object',
  templateUrl: './report-object-wrapper.component.html',
  styleUrls: ['./report-object-wrapper.component.scss']
})
export class ReportObjectWrapperComponent implements OnInit {

  public reportObjectType = ReportObjectType;

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log(this.ro);
  }

}
