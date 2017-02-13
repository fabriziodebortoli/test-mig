import { Component, OnInit, Input } from '@angular/core';
import { TbComponent } from './../../..';
import { HttpService, DocumentService, WebSocketService } from 'tb-core';

@Component({
  selector: 'tb-toolbar-top-button',
  templateUrl: './toolbar-top-button.component.html',
  styleUrls: ['./toolbar-top-button.component.scss']
})
export class ToolbarTopButtonComponent extends TbComponent implements OnInit {

  @Input() caption: string = '';
  @Input() icon: string = '';

  constructor(
    private webSocket: WebSocketService,
    private document: DocumentService,
    private httpService: HttpService
  ) {
    super();
  }

  ngOnInit() {
  }
  getIconUrl() {
    return this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + this.icon;
  }
  onCommand() {
    this.webSocket.doCommand(this.document.mainCmpId, this.cmpId, this.document.model);
  }
}