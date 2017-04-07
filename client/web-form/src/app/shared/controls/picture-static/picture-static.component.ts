import { Component, Input } from '@angular/core';

import { HttpService } from './../../../core/http.service';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-picture-static',
  templateUrl: './picture-static.component.html',
  styleUrls: ['./picture-static.component.scss']
})
export class PictureStaticComponent extends ControlComponent {

  @Input() width: string;
  @Input() height: string;

  constructor(private httpService: HttpService) {
    super();
  }

  getImageUrl(namespace: string) {
    return this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + namespace;
  }

}
