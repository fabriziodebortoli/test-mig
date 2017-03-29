import { Component, OnInit, Input } from '@angular/core';
// import { ReportObject } from './../../../reporting-studio.model';

@Component({
  selector: 'rs-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss']
})
export class ReportObjectImageComponent implements OnInit {

  // @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectImageComponent', this.ro);

  }

}
