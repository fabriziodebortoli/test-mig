import { Component, Input } from '@angular/core';

import { HttpService } from './../../../../services/http.service';
import { EventDataService } from './../../../../services/eventdata.service';

import { TbComponent } from './../../..';

@Component({
  selector: 'tb-toolbar-top-button',
  template: "<button kendoButton class=\"top-button\" (click)=\"onCommand()\" [disabled]=\"disabled\" title=\"{{caption}}\"> <tb-icon [iconType]=\"iconType\" [icon]=\"icon\"></tb-icon> <span>{{caption}}</span> </button>",
  styles: [".top-button { margin: 0; padding: 0; height: 30px; min-width: 30px; border: 0; background: none; } .top-button:hover { background: #f1f4f7; } .k-button > span { font-size: 12px; line-height: 30px; height: 30px; margin: 0; padding: 0 8px 0 0; display: none; } tb-icon { min-width: 30px; font-size: 20px; line-height: 34px; } md-icon { font-size: 30px; width: 30px; } .div-icon { width: 30px; height: 30px; line-height: 30px; } .div-icon img { height: 20px; margin-top: 4px; } "]
})
export class ToolbarTopButtonComponent extends TbComponent {

  @Input() caption: string = '';
  @Input() disabled: boolean = false;

  @Input() iconType: string = 'IMG'; // MD, TB, CLASS, IMG  
  @Input() icon: string = '';

  imgUrl: string;

  constructor(
    private eventData: EventDataService,
    private httpService: HttpService
  ) {
    super();

    this.imgUrl = this.httpService.getDocumentBaseUrl() + 'getImage/?src=';
  }

  onCommand() {
    this.eventData.command.emit(this.cmpId);
  }
}