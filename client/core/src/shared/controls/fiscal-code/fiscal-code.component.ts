import { ControlContainerComponent } from './../control-container/control-container.component';
import { Component, Input, ViewChild } from '@angular/core';
import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-fiscal-code',
    templateUrl: './fiscal-code.component.html',
    styleUrls: ['./fiscal-code.component.scss']
})
export class FiscalCodeComponent extends ControlComponent {
    @Input() disabled: boolean;
    @Input() mask: string;

    @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

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
        this.cc.errorMessage = "";
        if (value.length == 16) return true;
        this.cc.errorMessage = 'Fiscal code must be 16 chars';
        return false;
    }
}
