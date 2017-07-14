import { Component, Input, ViewEncapsulation, Output, EventEmitter } from '@angular/core';

import { TbComponent } from "../components";

@Component({
    template: ''
})
export class ControlComponent extends TbComponent {
    private _model: any;

    private _width: number;
    private _height: number;


    @Input()
    public caption: string;
    @Input()
    public args: any;
    @Input()
    public validators: Array<any> = [];
    @Input()
    public value: any;

    @Output('blur') blur: EventEmitter<any> = new EventEmitter();


    get width(): number {
        return this._width;
    }

    @Input()
    set width(width: number) {
        this._width = width;
    }


    get height(): number {
        return this._height;
    }

    @Input()
    set height(height: number) {
        this._height = height;
    }


    get model(): any {
        return this._model;
    }

    @Input()
    set model(model: any) {
        if (model == undefined)
            return;

        this._model = model;
        this.value = model.value;
    }
}
