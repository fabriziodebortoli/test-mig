import { Component, OnInit, Input } from '@angular/core';

import { DocumentService } from '../../core/document.service';
import { WebSocketService } from '../../core/websocket.service';

@Component({
  selector: 'tb-toolbar-button',
  template: `<div (click)='onCommand()' title='{{caption}}'><md-icon>{{icon}}</md-icon></div>`,
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
export class ToolbarButtonComponent implements OnInit {

  @Input() caption: string = '';
  @Input() cmd: string = '';
  @Input() icon: string = 'tag_faces';

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