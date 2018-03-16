import { Component, Input } from '@angular/core';

import { InfoService } from './../../../core/services/info.service';

@Component({
    selector: 'tb-icon',
    templateUrl: './tb-icon.component.html'
})
export class TbIconComponent {

    @Input() iconType: string = 'IMG'; // TB, CLASS, IMG  

    @Input() _icon: string = '';

    @Input()
    set icon(icon: any) {
        this._icon = icon instanceof Object ? icon.value : icon;
    }

    get icon() {
        return this._icon;
    }


    imgUrl: string;

    constructor(public infoService: InfoService) {
        this.imgUrl = this.infoService.getDocumentBaseUrl() + 'getImage/?src=';
    }

}
