import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-vat-code',
    templateUrl: './vat-code.component.html',
    styleUrls: ['./vat-code.component.scss']
})
export class VATCodeComponent extends ControlComponent {
    @Input() forCmpID: string;
    @Input() disabled: boolean;
    @Input() mask: string;
}
