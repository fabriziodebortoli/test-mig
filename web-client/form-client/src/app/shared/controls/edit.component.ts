import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-edit',
     template: `<div>
     <label>{{caption}}</label>
     <input id="{{cmpId}}" type="text" [disabled] = "!model?.enabled" [ngModel]="model?.value" (ngModelChange)="model.value=$event" />
     </div>`
})

export class EditComponent extends TbComponent {
    @Input()
    public caption: string;
    @Input()
    public model: any;
}
