import { ComponentService } from './../../../core/component.service';
import { link } from './../../reporting-studio.model';
import { HttpService } from './../../../core/http.service';
import { Component, Input } from '@angular/core';
import { LinkType } from './../../reporting-studio.model';


@Component({
  selector: 'rs-link',
  templateUrl: './link.component.html',
  styleUrls: ['./link.component.scss']
})
export class ReportLinkComponent {

  @Input() link: link;

  constructor(private httpService: HttpService, private componentService: ComponentService) { }

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
    const params = { xmlArgs : encodeURIComponent(this.link.arguments), runAtTbLoader: false};
    this.componentService.createReportComponent(this.link.ns, true, params);
  }

  openDocument() {
   this.httpService.runDocument(this.link.ns, this.link.arguments);
  }

  openLink() {
    const link = decodeURIComponent(this.link.ns);
    window.open(link, '_blank');
  }
}