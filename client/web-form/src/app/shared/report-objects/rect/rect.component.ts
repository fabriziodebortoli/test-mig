import { sqrrect } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'rs-rect',
  templateUrl: './rect.component.html',
  styleUrls: ['./rect.component.scss']
})
export class ReportRectComponent {

  @Input() rect: sqrrect;

  constructor() { }
  
  applyStyle(): any {

    let obj = {
      'background-color': this.rect.bkgcolor,
      'border-left': this.rect.borders.left ? this.rect.pen.width + 'px' : '0px',
      'border-right': this.rect.borders.right ? this.rect.pen.width + 'px' : '0px',
      'border-bottom': this.rect.borders.bottom ? this.rect.pen.width + 'px' : '0px',
      'border-top': this.rect.borders.top ? this.rect.pen.width + 'px' : '0px',
      'border-style': 'solid',
       'border-color': 'background-color',
      'position': 'absolute',
      'top': this.rect.rect.top + 'px',
      'left': this.rect.rect.left + 'px',
      'width': this.rect.rect.right - this.rect.rect.left + 'px',
      'height': this.rect.rect.bottom - this.rect.rect.top + 'px',
      'margin': '1em',
      'border-radius': this.rect.ratio + 'px',
      'box-shadow': this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_height + 'px ' + this.rect.shadow_color
    };

    return obj;
  }

}
