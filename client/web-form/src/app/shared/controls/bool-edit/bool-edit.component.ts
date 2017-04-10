import { Component, Input } from '@angular/core';
import { HttpService } from './../../../core/http.service';
import { EventDataService } from './../../../core/eventdata.service';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-bool-edit',
  templateUrl: './bool-edit.component.html',
  styleUrls: ['./bool-edit.component.scss']
})
export class BoolEditComponent extends ControlComponent {

  @Input() yesFirst:string;
  @Input() noFirst:string;

  constructor(private eventData: EventDataService) {
    super();
  }

  keyPress(event) {

    // sanitizing input
    if (
      this.yesFirst == null || 
      this.noFirst == null ||
      this.yesFirst.length > 1 ||
      this.noFirst.length > 1) {
      event.preventDefault();
      return;
    }

    let localizedYes:any = 'Key' + this.yesFirst.toUpperCase();
    let localizedNo:any = 'Key' + this.noFirst.toUpperCase();

    if (
      event.code != localizedYes &&
      event.code != localizedNo
      ) {
      event.preventDefault();
      return;
    }
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }  
}
