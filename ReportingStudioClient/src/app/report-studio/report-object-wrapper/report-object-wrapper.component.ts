import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './report-object';

@Component({
  selector: 'rs-report-object',
  templateUrl: './report-object-wrapper.component.html',
  styleUrls: ['./report-object-wrapper.component.css']
})
export class ReportObjectWrapperComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    console.log(this.ro);
  }

}
