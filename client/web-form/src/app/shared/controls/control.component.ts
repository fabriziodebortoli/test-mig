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
}
