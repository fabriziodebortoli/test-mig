import { Component } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-checkbox',
    template: "<div class=\"tb-control tb-checkbox\"> <tb-caption caption=\"{{caption}}\" [for]=\"cmpId\"></tb-caption> <input type=\"checkbox\" id=\"{{cmpId}}\" [ngModel]=\"model?.value\" [disabled]=\"!model?.enabled\" (ngModelChange)=\"model.value=$event\" class=\"k-checkbox\"> <label class=\"k-checkbox-label\" for=\"{{cmpId}}\">&nbsp;</label> </div>",
    styles: [".mat-checkbox-input { border: 1px solid red; } "]
})

export class CheckBoxComponent extends ControlComponent {
}
