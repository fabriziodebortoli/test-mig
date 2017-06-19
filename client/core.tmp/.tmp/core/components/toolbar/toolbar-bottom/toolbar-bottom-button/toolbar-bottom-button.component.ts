import { Component, OnInit, Input } from '@angular/core';

import { EventDataService } from './../../../../services/eventdata.service';

@Component({
  selector: 'tb-toolbar-bottom-button',
  template: "<button kendoButton title='{{caption}}' (click)='onCommand()' [disabled]=\"disabled\">{{caption}}</button>",
  styles: ["button { cursor: pointer; margin: 0 6px 0 0px; background: #065aad; color: #fff; padding: 0 15px; line-height: 30px; border-radius: 5px; font-size: 14px; font-weight: 300; border: 0; } button:hover { background: #003a73; } "]
})
export class ToolbarBottomButtonComponent implements OnInit {

  @Input() caption = '--unknown--';
  @Input() cmpId = '';
  @Input() disabled = false;

  constructor(private eventData: EventDataService) { }

  ngOnInit() {
  }

  onCommand() {
    this.eventData.command.emit(this.cmpId);
  }
}