import { EventDataService } from './../../../core/eventdata.service';
import { Component, Input } from '@angular/core';
import { ControlComponent } from "../control.component";

@Component({
  selector: 'tb-text',
  templateUrl: './text.component.html',
  styleUrls: ['./text.component.scss']
})
export class TextComponent extends ControlComponent {

  constructor(private eventData: EventDataService) {
    super();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }
}
