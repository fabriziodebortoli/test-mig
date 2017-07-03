import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

import { HttpService } from './../../services/http.service';

@Component({
  selector: 'tb-image',
  template: "<div [title]=\"title\"> <img id=\"{{cmpId}}\" [src]=\"getImageUrl(model?.value)\" *ngIf=\"model\" class=\"tb-static-component\" [ngStyle]=\"getStyles()\" /> </div>",
  styles: [""]
})
export class ImageComponent extends ControlComponent {

  @Input() width: number;
  @Input() height: number;
  @Input() title: string = '';

  constructor(private httpService: HttpService) {
    super();
  }

  getStyles() {

    let imgStyles = {};

    if (+(this.width) > +(this.height)) {
      imgStyles['width'] = this.width + 'px';
    } else {
      imgStyles['height'] = this.height + 'px';
    }

    return imgStyles;
  }

  getImageUrl(namespace: string) {
    return this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
  }

}
