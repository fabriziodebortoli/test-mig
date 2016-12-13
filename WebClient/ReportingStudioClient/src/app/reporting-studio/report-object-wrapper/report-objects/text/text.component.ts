import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../../report.model';

@Component({
  selector: 'rs-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.css']
})
export class ReportObjectTextComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectTextComponent', this.ro);
  }

}
