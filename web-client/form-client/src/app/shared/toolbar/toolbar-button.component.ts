import { DocumentService } from './../../core/document.service';
import { WebSocketService } from './../../core/websocket.service';

import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-button',
  template: `<a href='javascript:void(0)' (click)='onCommand()'>{{caption}}</a>`
})
export class ToolbarButtonComponent implements OnInit {

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