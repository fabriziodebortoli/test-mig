import { baseobj } from './../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';
import { ReportObjectType } from '../reporting-studio.model';

@Component({
  selector: 'rs-report-object',
  templateUrl: './report-object-wrapper.component.html',
  styleUrls: ['./report-object-wrapper.component.scss']
})
export class ReportObjectWrapperComponent implements OnInit {

  public reportObjectType = ReportObjectType;

  @Input() ro: baseobj;

  constructor() { }

  ngOnInit() {
    // console.log(this.ro);
  }

}
