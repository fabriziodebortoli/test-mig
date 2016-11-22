import { ReportObject } from './report-object-wrapper/report-object';
import { Component, OnInit, Input } from '@angular/core';


@Component({
  selector: 'rs-report-generator',
  templateUrl: './report-generator.component.html'
})
export class ReportGeneratorComponent implements OnInit {

  @Input() report: ReportObject[];

  constructor() { }

  ngOnInit() {
  }

}
