import { EventService } from 'tb-core';
import { ControlComponent } from './control.component';
import { Component, Input } from '@angular/core';


@Component({
    selector: 'tb-edit',
     template: `<div>
     <md-input id="{{cmpId}}" type="text" (blur)="onBlur()"  [disabled] = "!model?.enabled" [ngModel]="model?.value" (ngModelChange)="model.value=$event" [placeholder]="caption" ></md-input>
     </div>`
})

export class EditComponent extends ControlComponent{
    
    constructor(
        private events: EventService
      ) {
        super();
      }

    onBlur() {
        this.events.change.emit(this.cmpId);
       
    }
}
