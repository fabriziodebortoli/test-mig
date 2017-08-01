import { chart } from './../../../models/chart.model';
import { UtilsService } from '@taskbuilder/core';

import { Component, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';

@Component({
  selector: 'rs-chart',
  templateUrl: './chart.component.html',
  styles: []
})
export class ReportChartComponent implements AfterViewInit {

  @Input() chart: chart;

  constructor(private cdRef: ChangeDetectorRef, private utils: UtilsService) {
  }

  ngAfterViewInit() {
    this.cdRef.detectChanges();
  }

  applyStyle(): any {
  
    let obj = {
      'position': 'absolute',
      'top': this.chart.rect.top + 'px',
      'left': this.chart.rect.left + 'px',
      'height': this.chart.rect.bottom - this.chart.rect.top + 'px',
      'width': this.chart.rect.right - this.chart.rect.left + 'px',
      'box-shadow': this.chart.shadow_height + 'px ' + this.chart.shadow_height + 'px '
      + this.chart.shadow_height + 'px ' + this.chart.shadow_color

    };
    return obj;
  }
/*
  applyValueStyle(): any {
    let lineHeight = 1 + 'px';
    if (this.rect.vertical_align === 'bottom') {
      // tslint:disable-next-line:max-line-length
      lineHeight = this.rect.label ? ((this.rect.rect.bottom - this.rect.rect.top) + (this.rect.rect.bottom - this.rect.rect.top) / 2) / 2 - 4 + 'px' :
        (this.rect.rect.bottom - this.rect.rect.top - 2) + (this.rect.rect.bottom - this.rect.rect.top - 2) / 2 - 4 + 'px';
    }
    if (this.rect.vertical_align === 'top') {
      // tslint:disable-next-line:max-line-length
      lineHeight = this.rect.label ? ((this.rect.rect.bottom - this.rect.rect.top - 2) - (this.rect.rect.bottom - this.rect.rect.top - 2) / 2) / 2 - 4 + 'px' :
        (this.rect.rect.bottom - this.rect.rect.top - 2) - (this.rect.rect.bottom - this.rect.rect.top - 2) / 2 - 4 + 'px';
    }
    else if (this.rect.vertical_align === 'middle') {
      lineHeight = this.rect.label ? (this.rect.rect.bottom - this.rect.rect.top) / 2 + 'px' :
        (this.rect.rect.bottom - this.rect.rect.top) + 'px';
    }

    let obj = {
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'position': 'relative',
      'display': 'block',
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'color': this.rect.textcolor,
      'text-align': this.rect.text_align,
      'line-height': lineHeight
    };
    return obj;
  }

  applyLabelStyle(): any {

    let obj = {
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'position': 'relative',
      'display': 'block',
      'overflow': 'hidden',
      'white-space': 'nowrap',
      'text-align': this.rect.label.text_align,
      'font-family': this.rect.label.font.face,
      'font-size': this.rect.label.font.size + 'px',
      'font-style': this.rect.label.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.label.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.label.font.underline ? 'underline' : 'none',
      'color': this.rect.label.textcolor,
    };
    return obj;
  }
*/


}