import { Component, Input } from '@angular/core';

import { HttpService } from '@taskbuilder/core';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss']
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
