import { Component, Input, ChangeDetectorRef } from '@angular/core';

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
    public infoService: InfoService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef:ChangeDetectorRef) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  getStyles() {

    let imgStyles = {};

    if(this.width) imgStyles['max-width'] = this.width + 'px';
    if(this.height) imgStyles['max-height'] = this.height + 'px';

    return imgStyles;
  }

  getImageUrl(namespace: string) {
    return this.infoService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
  }

}
