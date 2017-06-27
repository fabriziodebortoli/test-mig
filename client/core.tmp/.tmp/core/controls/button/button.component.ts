import { Component } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-button',
    template: "<button kendoButton id=\"{{cmpId}}\">{{ caption }}</button>",
    styles: [""]
})
export class ButtonComponent extends ControlComponent { }
