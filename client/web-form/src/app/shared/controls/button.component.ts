import { ControlComponent } from './control.component';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-button',
    template: '<div><label>{{caption}}</label><input id="{{cmpId}}" type="button" [(ngModel)]="model"/></div>'
})

export class ButtonComponent  extends ControlComponent {
   
}
