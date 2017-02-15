import { TbComponent } from './../..';
import { HttpService, WebSocketService, EventService } from 'tb-core';
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
export class ToolbarTopButtonComponent extends TbComponent implements OnInit {

  @Input() caption: string = '';
  @Input() icon: string = '';

  constructor(
    private events: EventService,
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
    this.events.command.emit(this.cmpId);
  }
}