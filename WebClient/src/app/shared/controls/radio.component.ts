import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-radio',
    template: '<div><label>{{caption}}</label><input id="{{cmpId}}" type="radio" [(ngModel)]="model"/></div>'
})

export class RadioComponent  extends TbComponent{
    @Input()
    cmpId: string;
    @Input()
    caption: string;
    @Input()
    model: any;
}
