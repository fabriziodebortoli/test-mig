
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

  applyStyle(): any {

    let obj = {
      'background-color': this.rect.bkgcolor,
      'border-color': 'background-color',
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'position': 'absolute',
      'top': this.rect.rect.top + 'px',
      'left': this.rect.rect.left + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'margin': '1em'
    };

    return obj;
  }


  applyValueStyle(): any {
    let obj = {
      'color': this.rect.textcolor,
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'text-align': 'center'
    };

    return obj;
  }
}
