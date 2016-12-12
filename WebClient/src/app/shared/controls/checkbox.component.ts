import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-checkbox',
    template: '<div><label>{{caption}}</label><input id="{{cmpId}}" type="checkbox" [(ngModel)]="model"/></div>'
})

export class CheckBoxComponent  extends TbComponent{
    @Input()
    cmpId: string;
    @Input()
    caption: string;
    @Input()
    model: any;   
}
