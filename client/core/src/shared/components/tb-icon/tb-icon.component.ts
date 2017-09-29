import { Component, Input } from '@angular/core';

import { InfoService } from './../../../core/services/info.service';

@Component({
    selector: 'tb-icon',
    templateUrl: './tb-icon.component.html'
})
export class TbIconComponent {

    @Input() iconType: string = 'IMG'; // TB, CLASS, IMG  
    @Input() icon: string = '';

    imgUrl: string;

    constructor(private infoService: InfoService) {
        this.imgUrl = this.infoService.getDocumentBaseUrl() + 'getImage/?src=';
    }

}
