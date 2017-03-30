import { graphrect } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input } from '@angular/core';


@Component({
  selector: 'rs-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss']
})
export class ReportObjectImageComponent {

  @Input() image: graphrect;
  value = '';
  constructor() { };

  applyStyle(): any {
    let obj = {
      'position': 'absolute',
      'left': this.image.rect.left + 'px',
      'top': this.image.rect.top + 'px',
      'width': this.image.rect.right - this.image.rect.left + 'px',
      'height': this.image.rect.bottom - this.image.rect.top + 'px',
      'margin': '1em',
      'border-color': this.image.pen.color,
      'border-left': this.image.borders.left ? this.image.pen.width + 'px' : '0px',
      'border-right': this.image.borders.right ? this.image.pen.width + 'px' : '0px',
      'border-bottom': this.image.borders.bottom ? this.image.pen.width + 'px' : '0px',
      'border-top': this.image.borders.top ? this.image.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'border-radius': this.image.ratio + 'px',
      'box-shadow': this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_color
    };
    return obj;
  }

  applyImageStyle(): any {
    let obj = {
      'position': 'relative',
      'height': '100%',
      'width': '100%'
    };
    return obj;
  }

}
