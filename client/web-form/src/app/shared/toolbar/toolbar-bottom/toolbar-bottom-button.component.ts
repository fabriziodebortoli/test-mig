import { EventDataService } from './../../../core/eventdata.service';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-bottom-button',
  templateUrl: './toolbar-bottom-button.component.html',
  styleUrls: ['./toolbar-bottom-button.component.scss']
})
export class ToolbarBottomButtonComponent implements OnInit {

  @Input() caption: string = '--unknown--';
  @Input() cmpId: string = '';

  constructor(
    private eventData: EventDataService
  ) {
  }

  ngOnInit() {
  }

  onCommand() {
    this.eventData.command.emit(this.cmpId);
  }
}