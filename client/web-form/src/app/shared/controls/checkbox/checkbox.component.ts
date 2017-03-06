import { ControlComponent } from './../control.component';

import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-checkbox',
    templateUrl: 'checkbox.component.html',
    styleUrls: ['./checkbox.component.scss']
})

export class CheckBoxComponent  extends ControlComponent{
}
