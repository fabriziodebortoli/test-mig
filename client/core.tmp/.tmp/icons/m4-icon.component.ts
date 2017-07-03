import { Component, Input } from '@angular/core';

@Component({
    selector: 'm4-icon',
    styles: [""],
    template: `<i class="m4-icon m4-{{icon}}"></i>`
})
export class M4IconComponent {

    @Input('icon') icon = '';

    constructor() {
    }

}
