import { Component } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-button',
    templateUrl: 'button.component.html',
    styleUrls: ['./button.component.scss']
})
export class ButtonComponent extends ControlComponent { }
