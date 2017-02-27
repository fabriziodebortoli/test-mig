import { EventDataService } from './../../core/eventdata.service';
import { ControlComponent } from './control.component';
import { Component } from '@angular/core';


@Component({
    selector: 'tb-edit',
     template: `<div>
     <md-input id="{{cmpId}}" type="text" (blur)="onBlur()"  [disabled] = "!model?.enabled" [ngModel]="model?.value" (ngModelChange)="model.value=$event" [placeholder]="caption" ></md-input>
     </div>`
})

export class EditComponent extends ControlComponent{
    
    constructor(
        private eventData: EventDataService
      ) {
        super();
      }

    onBlur() {
        this.eventData.change.emit(this.cmpId);
       
    }
}
