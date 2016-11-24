import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../report-object.model';

@Component({
  selector: 'rs-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.css']
})
export class ReportObjectImageComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
  }

}
