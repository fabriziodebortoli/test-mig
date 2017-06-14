import { WebSocketService } from './../../../../core/websocket.service';
import { ComponentService } from './../../../../core/component.service';
import { link } from './../../../reporting-studio.model';
import { Component, Input } from '@angular/core';
import { LinkType } from './../../../reporting-studio.model';


@Component({
  selector: 'rs-link',
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss']
})
export class ReportLinkComponent {

  @Input() link: link;

  constructor(
    private componentService: ComponentService,
    private webSocketService: WebSocketService) { }

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