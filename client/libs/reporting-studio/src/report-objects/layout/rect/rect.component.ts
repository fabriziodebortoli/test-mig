import { sqrrect } from './../../../models/sqrrect.model';
import { UtilsService } from '@taskbuilder/core';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'rs-rect',
  templateUrl: './rect.component.html',
  styles: []
})
export class ReportRectComponent {

  @Input() rect: sqrrect;

  constructor(public utils: UtilsService) { }

  applyStyle(): any {
    let rgba = this.utils.hexToRgba(this.rect.bkgcolor);
    rgba.a = this.rect.transparent ? 0 : 1;
    let backgroundCol = 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
    let obj = {
      'background-color': backgroundCol,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'border-color': this.rect.pen.color,
      'box-sizing': 'border-box',
      'position': 'absolute',
      'top': this.rect.rect.top + 'px',
      'left': this.rect.rect.left + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'border-radius': this.rect.ratio + 'px',
      'box-shadow': this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_color
    };

    return obj;
  }

}
