import { TbComponent } from '..';
import { Component, Input } from '@angular/core';

@Component({
    template: ''
})

export class ControlComponent extends TbComponent {
    @Input()
    public caption: string;
    @Input()
    public model: any;
    @Input()
    public args: any;
    @Input()
    public validators: Array<any> = [];
}
