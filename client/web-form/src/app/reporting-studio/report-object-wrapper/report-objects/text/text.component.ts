import { Component, OnInit, Input } from '@angular/core';
import { ReportObject } from './../../../reporting-studio.model';

@Component({
  selector: 'rs-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class ReportObjectTextComponent implements OnInit {

  @Input() ro: ReportObject;

  constructor() { }

  ngOnInit() {
    // console.log('ReportObjectTextComponent', this.ro);
  }

}
