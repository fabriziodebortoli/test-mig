import { environment } from './../../../../environments/environment';

import { graphrect } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'rs-image',
  templateUrl: './image.component.html',
  styles: []
})
export class ReportImageComponent {

  @Input() image: graphrect;

  constructor() {

  };
  applyStyle(): any {
    let obj = {
      'position': 'absolute',
      'left': this.image.rect.left + 'px',
      'top': this.image.rect.top + 'px',
      'width': this.image.rect.right - this.image.rect.left + 'px',
      'height': this.image.rect.bottom - this.image.rect.top + 'px',
      'border-left': this.image.borders.left ? this.image.pen.width + 'px' : '0px',
      'border-right': this.image.borders.right ? this.image.pen.width + 'px' : '0px',
      'border-bottom': this.image.borders.bottom ? this.image.pen.width + 'px' : '0px',
      'border-top': this.image.borders.top ? this.image.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'border-color': this.image.pen.color,
      'border-radius': this.image.ratio + 'px',
      'box-shadow': this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_height + 'px ' + this.image.shadow_color
    };

    if (this.image.value !== '') {
      //this.image.src = 'http://www.jqueryscript.net/images/Simplest-Responsive-jQuery-Image-Lightbox-Plugin-simple-lightbox.jpg';
       this.image.src = environment.baseUrl + 'rs/image/' + this.image.value;
    }
    
    return obj;
  }

  applyImageStyle(): any {
    let obj = {
      'position': 'relative',
      'max-width': '100%',
      'max-height': '100%'
    };
    return obj;
  }

}
