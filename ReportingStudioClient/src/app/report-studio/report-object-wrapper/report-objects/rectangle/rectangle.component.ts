import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../report-object.model';

@Component({
  selector: 'rs-rectangle',
  templateUrl: './rectangle.component.html',
  styleUrls: ['./rectangle.component.css']
})
export class ReportObjectRectangleComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectRectangleComponent', this.ro);
  }

}
