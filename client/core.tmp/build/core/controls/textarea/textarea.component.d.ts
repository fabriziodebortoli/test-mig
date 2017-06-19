import { ControlComponent } from './../control.component';
export declare class TextareaComponent extends ControlComponent {
    readonly: boolean;
    width: number;
    height: number;
    constructor();
    getCorrectHeight(): string;
    getCorrectWidth(): string;
}
