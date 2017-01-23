import { HttpService, DocumentService, WebSocketService } from 'tb-core';
import { Component, OnInit, Input } from '@angular/core';


@Component({
  selector: 'tb-toolbar-top-button',
  template: `<div (click)='onCommand()' title='{{caption}}'><img src="{{getIconUrl()}}"/></div>`,
  styles: [`
    div{
      cursor:pointer;
      margin: 0 4px;
    }
    md-icon{
      line-height: 30px;
      display: block;
      font-size: 22px;
    }
  `]
})
export class ToolbarTopButtonComponent implements OnInit {

  @Input() caption: string = '';
  @Input() cmpId: string = '';
  @Input() icon: string = '';

  constructor(
    private webSocket: WebSocketService,
    private document: DocumentService,
    private httpService: HttpService
  ) {
  }

  ngOnInit() {
  }
  getIconUrl() {
    return this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + this.icon;
  }
  onCommand() {
    this.webSocket.doCommand(this.document.mainCmpId, this.cmpId);
  }
}