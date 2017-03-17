import { AfterViewInit } from 'libclient/node_modules/@angular/core';
import { fieldrect } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'rs-fieldrect',
  templateUrl: './fieldrect.component.html',
  styleUrls: ['./fieldrect.component.scss']
})
export class ReportFieldrectComponent implements AfterViewInit {

  @Input() rect: fieldrect;

  constructor(private cdRef: ChangeDetectorRef) {
  }

  ngAfterViewInit() {
    this.cdRef.detectChanges();
  }
}