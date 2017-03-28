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

  applyStyle(): any {
    let obj = {
      'position': 'absolute',
      'left': this.rect.rect.left + 'px',
      'top': this.rect.rect.top + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'margin': '1em'
    };
    return obj;
  }

  applyValueStyle(): any {
    let obj = {
      'position': 'relative',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'background-color': this.rect.bkgcolor,
      'border-color': this.rect.pen.color,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'color': this.rect.textcolor,
    };
    return obj;
  }

  applyLabelStyle(): any {
    let obj = {
      'position': 'relative',
      'text-align': 'right',
      'font-family': this.rect.label.font.face,
      'font-size': this.rect.label.font.size + 'px',
      'font-style': this.rect.label.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.label.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.label.font.underline ? 'underline' : 'none',
      'color': this.rect.label.textcolor,
    };
    return obj;
  }


}