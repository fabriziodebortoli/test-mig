import { textrect } from './../../../models/textrect.model';

import { UtilsService } from '@taskbuilder/core';

import { Component, Input, ChangeDetectorRef, AfterViewInit } from '@angular/core';


@Component({
  selector: 'rs-textrect',
  templateUrl: './textrect.component.html',
  styles: []
})
export class ReportTextrectComponent implements AfterViewInit {

  @Input() rect: textrect;

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
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'background-color': backgroundCol,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-color': this.rect.pen.color,
      'border-style': 'solid',
      'border-radius': this.rect.ratio + 'px',
      'box-sizing': 'border-box',
      'box-shadow': this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_color,

    };

    return obj;
  }


  applyValueStyle(): any {
    let obj = {
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'color': this.rect.font.fontcolor !== undefined ? this.rect.font.fontcolor : this.rect.textcolor,
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'text-align': this.rect.text_align,
      'transform': 'rotate(' + this.rect.rotateBy + 'deg)',
      'white-space': this.rect.line === 'single_line' ? 'nowrap' : 'pre-line',
      'display': 'flex',
      'overflow': 'hidden',
    };

    return obj;
  }

  applyDummyTableStyle(): any {
    let obj = {
      'text-align': this.rect.text_align,
      'margin-left': this.rect.text_align == 'left' ? '0px' : 'auto',
      'margin-right': this.rect.text_align == 'right' ? '0px' : 'auto',
      'height': 'inherit',
      'width': 'inherit',
      'table-layout': 'fixed',
      'border-spacing': '0px',
    };
    return obj;
  }

  applyDummyCellStyle(): any {
    let obj = {
      'width': 'inherit',
      'vertical-align': this.rect.vertical_align,
      'text-align': this.rect.text_align,
      'overflow':'hidden',
    };
    return obj;
  }
}
