import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, Input } from '@angular/core';

import { HttpService } from './../../../core/services/http.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss']
})
export class ImageComponent extends ControlComponent {

  @Input() title: string = '';

  constructor(private httpService: HttpService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService) {
    super(layoutService, tbComponentService);
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
