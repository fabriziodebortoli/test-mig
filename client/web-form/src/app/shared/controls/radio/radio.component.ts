import { ControlComponent } from './../control.component';
import { Component, Input, enableProdMode } from '@angular/core';

@Component({
    selector: 'tb-radio',
    templateUrl: 'radio.component.html',
    styleUrls: ['./radio.component.scss']
})

export class RadioComponent extends ControlComponent {
    @Input() name: string;
    @Input() value: string;
}
