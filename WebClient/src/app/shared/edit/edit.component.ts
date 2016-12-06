
import { Component, Input } from '@angular/core';
import { TbComponent } from './../tb.component';

@Component({
    selector: 'edit-cmp',
    template: '<div><label>{{caption}}</label><input id="{{cmpId}}" type="text" [(ngModel)]="model"/></div>'
})

export class EditComponent extends TbComponent  {
    @Input()
    cmpId: string;
    @Input()
    caption: string;
    @Input()
    model: any;   
}
