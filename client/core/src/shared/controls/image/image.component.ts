import { Component, Input } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { InfoService } from './../../../core/services/info.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-image',
  templateUrl: './image.component.html',
  styleUrls: ['./image.component.scss']
})
export class ImageComponent extends ControlComponent {

  @Input() title: string = '';

  constructor(
    private infoService: InfoService,
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
    return this.infoService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
  }

}
