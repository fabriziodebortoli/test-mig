import { Component, OnInit, Input } from '@angular/core';

import { EventDataService } from './../../../../services/eventdata.service';

@Component({
  selector: 'tb-toolbar-bottom-button',
  templateUrl: './toolbar-bottom-button.component.html',
  styleUrls: ['./toolbar-bottom-button.component.scss']
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