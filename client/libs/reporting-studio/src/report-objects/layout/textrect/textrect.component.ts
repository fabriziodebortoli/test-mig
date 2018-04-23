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
  widthValueStyle = 0;
  rotate: boolean = false;

  constructor(public cdRef: ChangeDetectorRef, public utils: UtilsService) {
  }

  ngAfterViewInit() {
    this.cdRef.detectChanges();
  }

  applyStyle(): any {
    let rgba = this.utils.hexToRgba(this.rect.bkgcolor);
    rgba.a = this.rect.transparent ? 0 : 1;
    let backgroundCol = 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
    let width = 0;
    let height = 0;
    let translate = '';
    if (this.rect.rotateBy.toString() === '0') {
      width = this.rect.rect.right - this.rect.rect.left;
      height = this.rect.rect.bottom - this.rect.rect.top;
    }
    else {
      width = this.rect.rect.bottom - this.rect.rect.top;
      height = this.rect.rect.right - this.rect.rect.left;
      if (this.rect.rotateBy.toString() === '270') {
        translate = 'translate(' + -(width - height) / 2 + 'px,' + -(width - height) / 2 + 'px)';
      }
      if (this.rect.rotateBy.toString() === '90') {
        translate = 'translate(' + (width - height) / 2 + 'px,' + (width - height) / 2 + 'px)';
      }
    }
    let obj = {
      'position': 'absolute',
      'top': this.rect.rect.top + 'px',
      'left': this.rect.rect.left + 'px',
      'width': width + 'px',
      'height': height + 'px',
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
      'transform': 'rotate(' + this.rect.rotateBy + 'deg)' + translate,
    };
    return obj;
  }

  applyValueStyle(): any {
    let width = 0;
    let height = 0;
    let borderWSize = (this.rect.borders.left ? this.rect.pen.width : 0) + (this.rect.borders.right ? this.rect.pen.width : 0);
    let borderHSize = (this.rect.borders.top ? this.rect.pen.width : 0) + (this.rect.borders.bottom ? this.rect.pen.width : 0);
    if (this.rect.rotateBy.toString() === '0') {
      width = this.rect.rect.right - this.rect.rect.left - borderWSize;
      height = this.rect.rect.bottom - this.rect.rect.top - borderHSize;
    }
    else {
      width = this.rect.rect.bottom - this.rect.rect.top - borderHSize;
      height = this.rect.rect.right - this.rect.rect.left - borderWSize;
    }

    let obj = {
      'width': width + 'px',
      'height': height + 'px',
      'color': this.rect.textcolor !== undefined && this.rect.textcolor !== '#000000'? this.rect.textcolor : this.rect.font.fontcolor,
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'text-align': this.rect.text_align,
      'white-space': this.rect.line === 'single_line' ? 'nowrap' : 'pre-line',
      'display': 'flex',
      'overflow': 'hidden',
    };
    return obj;
  }

  applyFieldSetStyle() {
    let rgba = this.utils.hexToRgba(this.rect.bkgcolor);
    rgba.a = this.rect.transparent ? 0 : 1;
    let backgroundCol = 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
    let obj = {
      'position': 'absolute',
      'top': this.rect.rect.top + 'px',
      'left': this.rect.rect.left + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + this.rect.font.size / 2 + 'px',
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
      'margin-top': - this.rect.font.size / 2 + 'px',
    };
    return obj;
  }

  applyContainerClass() {
    let obj = {
      'display': 'contents',
      'flex-direction': 'row',
      'flex-wrap': 'nowrap',
      'justify-content': 'flex-start',
      'height': 'inherit',
      'width': 'inherit',
      'margin-left': this.rect.text_align == 'left' ? '0px' : 'auto',
      'margin-right': this.rect.text_align == 'right' ? '0px' : 'auto',
      'table-layout': 'fixed',
      'border-spacing': '0px',
    }
    return obj;
  }

  applyItemClass() {
    let align_self = 'flex-start';
    let tAlign = this.rect.text_align;

    if (this.rect.rotateBy.toString() === '0') {
      if (this.rect.vertical_align == 'top')
        align_self = 'flex-start';
      else if (this.rect.vertical_align == 'middle')
        align_self = 'center';
      else if (this.rect.vertical_align == 'bottom')
        align_self = 'flex-end';
    }
    else if (this.rect.rotateBy.toString() === '270') {
      if (this.rect.text_align == 'right')
        align_self = 'flex-end';
      else if (this.rect.text_align == 'center')
        align_self = 'center';
      else if (this.rect.text_align == 'left')
        align_self = 'flex-start';

      if (this.rect.vertical_align == 'top')
        tAlign = 'right';
      else if (this.rect.vertical_align == 'middle')
        tAlign = 'center';
      else if (this.rect.vertical_align == 'bottom')
        tAlign = 'left';
    }
    else if (this.rect.rotateBy.toString() === '90') {
      if (this.rect.text_align == 'right')
        align_self = 'flex-start';
      else if (this.rect.text_align == 'center')
        align_self = 'center';
      else if (this.rect.text_align == 'left')
        align_self = 'flex-end';

      if (this.rect.vertical_align == 'top')
        tAlign = 'left';
      else if (this.rect.vertical_align == 'middle')
        tAlign = 'center';
      else if (this.rect.vertical_align == 'bottom')
        tAlign = 'right';
    }
    let obj = {
      'flex': '0 1 auto',
      'align-self': align_self,
      'width': 'inherit',
      'overflow': 'hidden',
      'line-height': 'initial',
      'text-align': tAlign,
      'padding': '0px 2px'
    }
    return obj;
  }

}
