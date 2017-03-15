import { textrect } from './../../../reporting-studio/reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-textrect',
  templateUrl: './textrect.component.html',
  styleUrls: ['./textrect.component.scss']
})
export class ReportTextrectComponent implements OnInit {
 @Input() rect: textrect;
  constructor() { }

  ngOnInit() {
  }

}
