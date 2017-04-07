import { link } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'rs-link',
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss']
})
export class ReportLinkComponent {

  @Input() link: link;
  constructor() { }

}