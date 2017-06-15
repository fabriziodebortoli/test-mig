import { Component, Input, Output, EventEmitter } from '@angular/core';

import { TbComponent } from '../components';

@Component({
    template: ''
})
export class ControlComponent extends TbComponent {
    private _model: any;

    @Input() public caption: string;
    @Input() public args: any;
    @Input() public validators: Array<any> = [];
    @Input() public value: any;

    @Output('blur') blur: EventEmitter<any> = new EventEmitter();

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
