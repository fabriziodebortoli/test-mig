import { Component, OnInit, Input } from '@angular/core';
// import { ReportObject } from './../../../reporting-studio/reporting-studio.model';

@Component({
  selector: 'rs-file',
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.scss']
})
export class ReportObjectFileComponent implements OnInit {

  // @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectFileComponent', this.ro);
  }

}
