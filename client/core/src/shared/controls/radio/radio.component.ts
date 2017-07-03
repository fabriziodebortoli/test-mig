import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-radio',
    templateUrl: 'radio.component.html',
    styleUrls: ['./radio.component.scss']
})

export class RadioComponent extends ControlComponent {
    @Input() name: string;
}
