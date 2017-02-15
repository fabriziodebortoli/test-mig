import { EventService } from 'tb-core';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-bottom-button',
  template: `<div (click)='onCommand()' title='{{caption}}'>{{caption}}</div>`,
  styles: [`
    div{
      cursor: pointer;
      margin: 5px 6px 0 0px;
      background: #065aad;
      color: #fff;
      padding: 0 15px;
      line-height: 30px;
      border-radius: 5px;
      font-size: 14px;
      font-weight: 300;
    }
    div:hover{
      background:#003a73;
    }
  `]
})
export class ToolbarBottomButtonComponent implements OnInit {

  @Input() caption: string = '--unknown--';
  @Input() cmpId: string = '';

  constructor(
    private events: EventService
  ) {
  }

  ngOnInit() {
  }

  onCommand() {
    this.events.command.emit(this.cmpId);
  }
}