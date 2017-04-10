import { Component, Input, ViewContainerRef, ViewChild } from '@angular/core';

import { HttpService } from './../../../core/http.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-picture-static',
  templateUrl: './picture-static.component.html',
  styleUrls: ['./picture-static.component.scss']
})
export class PictureStaticComponent extends ControlComponent {

  @Input() width: number;
  @Input() height: number;

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
