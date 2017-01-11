import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-edit',
     template: `<div>
     <md-input id="{{cmpId}}" type="text" [disabled] = "!model?.enabled" [ngModel]="model?.value" (ngModelChange)="model.value=$event" [placeholder]="caption" ></md-input>
     </div>`
})

export class EditComponent extends TbComponent {
    @Input()
    public caption: string;
    @Input()
    public model: any;
}
