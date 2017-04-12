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

  @Input() yesText:string;
  @Input() noText:string;

  constructor(private eventData: EventDataService) {
    super();

    if (
      this.yesText == null || 
      this.noText == null ||
      this.yesText.length == 0 ||
      this.noText.length == 0)
    {
      this.yesText = 'YES';
      this.noText = 'NO';
    }    
  }

  keyPress(event) {

    let firstYes:string = this.yesText[0].toUpperCase();
    let localizedCodeYes:any = 'Key' + firstYes;
    let localizedCodeNo:any = 'Key' + this.noText[0].toUpperCase();

    if (
      event.code != localizedCodeYes &&
      event.code != localizedCodeNo
      ) {
      return;
    }

    event.preventDefault();

    if (this.model == undefined)
      return;

    this.model.value = event.key.toUpperCase() == firstYes ?
      this.yesText.toUpperCase() : this.noText.toUpperCase();
  }

  onBlur() {
    this.eventData.change.emit(this.cmpId);
  }  
}
