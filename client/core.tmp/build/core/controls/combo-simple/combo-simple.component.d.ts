import { EventEmitter } from '@angular/core';
import { ControlComponent } from './../control.component';
export declare class ComboSimpleComponent extends ControlComponent {
    items: Array<any>;
    defaultItem: any;
    changed: EventEmitter<any>;
    selectionChange(value: any): void;
}
