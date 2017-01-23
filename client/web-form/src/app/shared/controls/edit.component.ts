import { ControlComponent } from './control.component';
import { Component, Input } from '@angular/core';

import { DocumentService } from '../../core/document.service';
import { WebSocketService } from '../../core/websocket.service';

@Component({
    selector: 'tb-edit',
     template: `<div>
     <md-input id="{{cmpId}}" type="text" (blur)="onBlur()"  [disabled] = "!model?.enabled" [ngModel]="model?.value" (ngModelChange)="model.value=$event" [placeholder]="caption" ></md-input>
     </div>`
})

export class EditComponent extends ControlComponent{
    @Input()
    public caption: string;
    @Input()
    public model: any;
    @Input() cmpId: string = '';

    constructor(
        private webSocket: WebSocketService,
        private document: DocumentService
      ) {
        super();
      }

    onBlur() {
        this.webSocket.doValueChanged(this.document.mainCmpId, this.cmpId, this.document.model);
    }
}
