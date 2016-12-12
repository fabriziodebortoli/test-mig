import { TbComponent } from '..';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-combo',
     template: '<div><label>{{caption}}</label><select id="{{cmpId}}" [(ngModel)]="model"></select></div>'
})
export class ComboComponent  extends TbComponent{
    @Input()
    cmpId: string;
    @Input()
    caption: string;
    @Input()
    model: any;
}
