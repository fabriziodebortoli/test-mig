import { DocumentService, WebSocketService } from 'tb-core';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-topbar-button',
  template: `<a href='javascript:void(0)' (click)='onCommand()'>{{caption}}</a>`
})
export class TopbarButtonComponent implements OnInit {

  @Input() caption: string = '';
  @Input() cmd: string = '';

  constructor(
    private webSocket: WebSocketService,
    private document: DocumentService
  ) {

  }

  ngOnInit() {
  }

  onCommand() {
    this.webSocket.doCommand(this.document.mainCmpId, this.cmd);
  }
}