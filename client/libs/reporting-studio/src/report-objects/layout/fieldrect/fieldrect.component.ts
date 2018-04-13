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
      //'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      //'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'background-color': backgroundCol,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-color': this.rect.pen.color,
      'border-radius': this.rect.ratio + 'px',
      'border-style': 'solid',
      'box-sizing': 'border-box',
      'box-shadow': this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px '
      + this.rect.shadow_height + 'px ' + this.rect.shadow_color

    };
    return obj;
  }

  applyValueStyle(): any {
    let borderWSize = (this.rect.borders.left ? this.rect.pen.width : 0) + (this.rect.borders.right ? this.rect.pen.width : 0) ;
    let borderHSize = (this.rect.borders.top ? this.rect.pen.width : 0) + (this.rect.borders.bottom ? this.rect.pen.width : 0) ;
    let obj = {
      'width': this.rect.rect.right - this.rect.rect.left - borderWSize + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top - borderHSize + 'px',  
      'font-family': this.rect.font.face,
      'font-size': this.rect.font.size + 'px',
      'font-style': this.rect.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.font.bold ? 'bold' : 'normal',
      'text-decoration': this.rect.font.underline ? 'underline' : 'none',
      'color': this.rect.font.fontcolor !== undefined ? this.rect.font.fontcolor : this.rect.textcolor,
      'transform': 'rotate(' + this.rect.rotateBy + 'deg)',
      'white-space': this.rect.line === 'single_line' ? 'nowrap' : 'pre-line',
      'display': 'flex'
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
      'padding-right' : this.rect.text_align == 'right'?'2px':'0px',
      'padding-left' : this.rect.text_align == 'left'?'2px':'0px',
    };
    return obj;
  }

  applyContainerClass() {
    let align_items = 'flex-start';
    if (this.rect.vertical_align == 'top')
      align_items = 'flex-start';
    else if (this.rect.vertical_align == 'center')
      align_items = 'center';
    else if (this.rect.vertical_align == 'bottom')
      align_items = 'flex-end';

    let obj = {
      'display': 'contents',
      'flex-direction': 'row',
      'flex-wrap': 'nowrap',
      'justify-content': 'flex-start',
      'align-items': align_items,
      'align-content': align_items,
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'margin-left': this.rect.text_align == 'left' ? '0px' : 'auto',
      'margin-right': this.rect.text_align == 'right' ? '0px' : 'auto',
    }
    return obj;
  }

  applyItemClass() {
    let align_self = 'auto';
    if (this.rect.vertical_align == 'top')
      align_self = 'flex-start';
    else if (this.rect.vertical_align == 'middle')
      align_self = 'center';
    else if (this.rect.vertical_align == 'bottom')
      align_self = 'flex-end';

   let obj = {
      'flex': '0 1 auto',
      'align-self': align_self,
      //'height': 'inherit',
      'width': 'inherit',
      'overflow': 'hidden',
      'line-height': 'initial',
      'text-align': this.rect.text_align,
      'vertical-align': this.rect.vertical_align
    }
    return obj;
  }

  applyLabelStyle(): any {
    let borderWSize = (this.rect.borders.left ? this.rect.pen.width : 0) + (this.rect.borders.right ? this.rect.pen.width : 0) ;
    let borderHSize = (this.rect.borders.top ? this.rect.pen.width : 0) + (this.rect.borders.bottom ? this.rect.pen.width : 0) ;
    let obj = {
      'width': this.rect.rect.right - this.rect.rect.left - borderWSize + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top - borderHSize + 'px',
      'font-family': this.rect.label.font.face,
      'font-size': this.rect.label.font.size + 'px',
      'font-style': this.rect.label.font.italic ? 'italic' : 'normal',
      'font-weight': this.rect.label.font.bold ? 'bold' : 'normal',
      'position': 'absolute',
      'overflow': 'hidden',
      'text-align': this.rect.label.text_align,
      'transform': 'rotate('+this.rect.label.rotateBy+'deg)',  
      'text-decoration': this.rect.label.font.underline ? 'underline' : 'none',
      'color': this.rect.label.textcolor,
      'white-space': this.rect.label.line === 'single_line' ? 'nowrap' : 'pre-line',
      'display': 'flex'
    };
    return obj;
  }

  applyDummyCellLabelStyle(): any {
    let obj = {
      'width': 'inherit',
      'vertical-align': this.rect.label.vertical_align,
      'text-align': this.rect.label.text_align,
      'overflow':'hidden', 
      'padding-right' : this.rect.label.text_align == 'right'?'2px':'0px',
      'padding-left' : this.rect.label.text_align == 'left'?'2px':'0px',
    };
    return obj;
  }
  applyDummyTableLabelStyle(): any {
    let obj = {
      'text-align': this.rect.label.text_align,
      'margin-left': this.rect.label.text_align == 'left' ? '0px' : 'auto',
      'margin-right': this.rect.label.text_align == 'right' ? '0px' : 'auto',
      'height': 'inherit',
      'table-layout': 'fixed',
      'border-spacing': '0px',
    };
    return obj;
  }



  applyLabelContainerClass() {
    let align_items = 'flex-start';
    if (this.rect.vertical_align == 'top')
      align_items = 'flex-start';
    else if (this.rect.vertical_align == 'center')
      align_items = 'center';
    else if (this.rect.vertical_align == 'bottom')
      align_items = 'flex-end';

    let obj = {
      'display': 'contents',
      'flex-direction': 'row',
      'flex-wrap': 'nowrap',
      'justify-content': 'flex-start',
      'align-items': align_items,
      'align-content': align_items,
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'margin-left': this.rect.label.text_align == 'left' ? '0px' : 'auto',
      'margin-right': this.rect.label.text_align == 'right' ? '0px' : 'auto',
    }
    return obj;
  }

  applyLabelItemClass() {
    let align_self = 'auto';
    if (this.rect.label.vertical_align == 'top')
      align_self = 'flex-start';
    else if (this.rect.label.vertical_align == 'middle')
      align_self = 'center';
    else if (this.rect.label.vertical_align == 'bottom')
      align_self = 'flex-end';

    let obj = {
      'flex': '0 1 auto',
      'align-self': align_self,
      //'height': 'inherit',
      'width': 'inherit',
      'overflow': 'hidden',
      'line-height': 'initial',
      'text-align': this.rect.label.text_align,
      'vertical-align': this.rect.label.vertical_align
    }
    return obj;
  }

  applyLinkContainerClass() {
    let align_items = 'flex-start';
    if (this.rect.vertical_align == 'top')
      align_items = 'flex-start';
    else if (this.rect.vertical_align == 'center')
      align_items = 'center';
    else if (this.rect.vertical_align == 'bottom')
      align_items = 'flex-end';

    let obj = {
      'display': 'inline-grid',
      'flex-direction': 'row',
      'flex-wrap': 'nowrap',
      'justify-content': 'flex-start',
      'align-items': align_items,
      'align-content': align_items,
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'margin-left': this.rect.text_align == 'left' ? '0px' : 'auto',
      'margin-right': this.rect.text_align == 'right' ? '0px' : 'auto',
    }
    return obj;
  }

  applyLinkItemClass() {
    let align_self = 'auto';
    if (this.rect.vertical_align == 'top')
      align_self = 'flex-start';
    else if (this.rect.vertical_align == 'middle')
      align_self = 'center';
    else if (this.rect.vertical_align == 'bottom')
      align_self = 'flex-end';

   let obj = {
      'flex': '0 1 auto',
      'align-self': align_self,
      //'height': 'inherit',
      'width': 'inherit',
      'overflow': 'hidden',
      'line-height': 'initial',
      'text-align': this.rect.text_align,
      'vertical-align': this.rect.vertical_align
    }
    return obj;
  }





}