import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-checkbox',
    templateUrl: 'checkbox.component.html',
    styleUrls: ['./checkbox.component.scss']
})

export class CheckBoxComponent  extends ControlComponent{
}
