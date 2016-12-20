import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-edit',
     template: '<div><label>{{caption}}</label><input id="{{cmpId}}" type="text" [(ngModel)]="model" [disabled]="!enabled"/></div>'
})

export class EditComponent extends TbComponent {
    @Input()
    public caption: string;
    @Input()
    public model: any;
    @Input()
    public enabled: boolean;
}
