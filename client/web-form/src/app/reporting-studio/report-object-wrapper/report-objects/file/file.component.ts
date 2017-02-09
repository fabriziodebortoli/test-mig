import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../../report.model';

@Component({
  selector: 'rs-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.css']
})
export class ReportObjectFileComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectFileComponent', this.ro);
  }

}
