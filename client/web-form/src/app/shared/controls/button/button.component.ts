import { ControlComponent } from './../control.component';
import { Component, Input } from '@angular/core';


@Component({
    selector: 'tb-button',
    templateUrl: 'button.component.html',
    styleUrls: ['./button.component.scss']
})

export class ButtonComponent  extends ControlComponent {
}
