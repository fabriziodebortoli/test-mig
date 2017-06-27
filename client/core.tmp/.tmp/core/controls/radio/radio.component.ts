import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-radio',
    template: "<div class=\"tb-control tb-radio\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input class=\"k-radio\" id=\"{{cmpId}}\" type=\"radio\" [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\"> <label class=\"k-radio-label\" for=\"{{cmpId}}\">&nbsp;</label> </div>",
    styles: [""]
})

export class RadioComponent extends ControlComponent {
    @Input() name: string;
}
