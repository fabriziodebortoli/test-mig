import { Component, Input } from '@angular/core';

@Component({
    selector: 'tb-icon',
    template: `<i class="tb-icon tb-icon-{{icon}}"></i>`
})
export class IconComponent {

    @Input('icon') icon: string = '';

    constructor() {
    }

}
