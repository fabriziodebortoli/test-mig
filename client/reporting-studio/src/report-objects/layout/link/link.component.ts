import { link } from './../../../models/link.model';
import { WebSocketService } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';

import { Component, Input } from '@angular/core';
import { LinkType } from './../../../models/link-type.model';



@Component({
  selector: 'rs-link',
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss']
})
export class ReportLinkComponent {

  @Input() link: link;

  public clicked = false;

  constructor(
    public componentService: ComponentService,
    public webSocketService: WebSocketService) { }

  linkClicked() {
    switch (this.link.type) {
      case LinkType.report:
        this.runReport();
        break;
      case LinkType.document:
        this.openDocument();
        break;
      case LinkType.url:
        this.openLink();
        break;
      default:
        break;
    }
    this.clicked = true;
  }

  runReport() {
    const params = { xmlArgs: encodeURIComponent(this.link.arguments), runAtTbLoader: false };
    this.componentService.createReportComponent(this.link.ns, true, params);
  }

  openDocument() {
    this.webSocketService.runDocument(this.link.ns, this.link.arguments);
  }

  openLink() {
    const link = decodeURIComponent(this.link.ns);
    window.open(link, '_blank');
  }
}