
import { textrect } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';


@Component({
  selector: 'rs-textrect',
  templateUrl: './textrect.component.html',
  styleUrls: ['./textrect.component.scss']
})
export class ReportTextrectComponent implements AfterViewInit {

  @Input() rect: textrect;

  constructor(private cdRef: ChangeDetectorRef) {
  }

  ngAfterViewInit() {
    this.cdRef.detectChanges();
  }
}
