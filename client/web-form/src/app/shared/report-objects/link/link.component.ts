import { ComponentService } from './../../../core/component.service';
import { HttpService } from './../../../core/http.service';
import { link } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input } from '@angular/core';
import { LinkType } from '../../../reporting-studio/reporting-studio.model';


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
    this.componentService.createComponentFromUrl('rs/reportingstudio/' + this.link.ns + '/' + this.link.arguments);
  }

  openDocument() {
   this.httpService.runDocument(this.link.ns, this.link.arguments);
  }

  openLink() {
    const link = decodeURIComponent(this.link.ns);
    window.open(link, '_blank');
  }
}