import { EventEmitter } from '@angular/core';
import { TbComponent } from '../components';
export declare class ControlComponent extends TbComponent {
    private _model;
    caption: string;
    args: any;
    validators: Array<any>;
    value: any;
    blur: EventEmitter<any>;
    model: any;
}
