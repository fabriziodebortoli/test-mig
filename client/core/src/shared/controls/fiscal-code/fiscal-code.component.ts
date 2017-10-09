import { Component, Input } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-fiscal-code',
    templateUrl: './fiscal-code.component.html',
    styleUrls: ['./fiscal-code.component.scss']
})
export class FiscalCodeComponent extends ControlComponent {
    @Input() disabled: boolean;
    @Input() mask: string;

    errorMessage = '';

    onBlur() {
        this.isValid(this.value);
    }

    isValid(value: string): boolean {
        if (!this.is16Chars(value)) return false;

        return true;
    }

    private isTaxIdNumber(value: string): boolean {
        return +value[0] >= 0;
    }

    private is16Chars(value: string): boolean {
        if (value.length == 16) return true;
        this.errorMessage = 'Fiscal code must be 16 chars';
        return false;
    }
}
