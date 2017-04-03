import { UtilsService } from './../../../core/utils.service';
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

  constructor(private cdRef: ChangeDetectorRef, private utils: UtilsService) {
  }

  ngAfterViewInit() {
    this.cdRef.detectChanges();
  }

  applyStyle(): any {
    let rgba = this.utils.hexToRgba(this.rect.bkgcolor);
    rgba.a = this.rect.transparent ? 0 : 1;
    let backgroundCol = 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
    let obj = {
      'position': 'absolute',
      'top': this.rect.rect.top + 'px',
      'left': this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'vertical-align': this.rect.vertical_align,
      'background-color': backgroundCol,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-color': this.rect.pen.color,
      'border-radius': this.rect.ratio + 'px',
      'border-style': 'solid',
      'box-shadow': this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_color,

    };
    return obj;
  }

  applyValueStyle(): any {
    let obj = {
      'position': 'relative',
      'height': '100%',
      'width': '100%',
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'color': this.rect.textcolor,
      'text-align': this.rect.text_align,
      'margin': '0 1em 0 1em',
    };
    return obj;
  }

  applyLabelStyle(): any {
    let obj = {
      'position': 'relative',
      'text-align': this.rect.label.text_align,
      'font-family': this.rect.label.font.face,
      'font-size': this.rect.label.font.size + 'px',
      'font-style': this.rect.label.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.label.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.label.font.underline ? 'underline' : 'none',
      'color': this.rect.label.textcolor,
      'margin': '0 0.5em 0 0.5em'
    };
    return obj;
  }



}