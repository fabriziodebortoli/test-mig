import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-button',
    template: '<div><label>{{caption}}</label><input id="{{cmpId}}" type="button" [(ngModel)]="model"/></div>'
})

export class ButtonComponent  extends TbComponent {
    @Input()
    public caption: string;
    @Input()
    public model: any;
}
