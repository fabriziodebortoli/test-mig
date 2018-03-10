import { fieldrect } from './../../../models/fieldrect.model';
import { UtilsService } from '@taskbuilder/core';

import { Component, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';

@Component({
  selector: 'rs-fieldrect',
  templateUrl: './fieldrect.component.html',
  styles: []
})
export class ReportFieldrectComponent implements AfterViewInit {

  @Input() rect: fieldrect;

  constructor(public cdRef: ChangeDetectorRef, public utils: UtilsService) {
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
      'background-color': backgroundCol,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-color': this.rect.pen.color,
      'border-radius': this.rect.ratio + 'px',
      'border-style': 'solid',

      'box-shadow': this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px '
      + this.rect.shadow_height + 'px ' + this.rect.shadow_color

    };
    return obj;
  }

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
    const regex: RegExp = new RegExp(/(\r\n|\n|\r)/);
    let obj = {
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',  
      'position': 'relative',
      //'display': 'block',
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'color': this.rect.font.fontcolor !== undefined ? this.rect.font.fontcolor : this.rect.textcolor,
      'text-align': this.rect.text_align,
      'transform': 'rotate(' + this.rect.rotateBy + 'deg)',
      //'line-height': lineHeight,
      'white-space': this.rect.value.match(regex) ? 'pre-line' : 'unset',
      'vertical-align':this.rect.vertical_align
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
      'transform': 'rotate('+this.rect.label.rotateBy+'deg)',
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