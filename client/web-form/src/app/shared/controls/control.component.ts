import { TbComponent } from '..';
import { Component, Input } from '@angular/core';

@Component({
    template: ''
})

export class ControlComponent extends TbComponent {
    private _model: any;

    @Input()
    public caption: string;
    @Input()
    public args: any;
    @Input()
    public validators: Array<any> = [];
    @Input()
    public value: any;

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
