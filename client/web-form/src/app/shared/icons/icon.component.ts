import { Component, Input } from '@angular/core';
import { HttpService } from './../../core/http.service';

@Component({
    selector: 'tb-icon',
    templateUrl: './icon.component.html'
})
export class IconComponent {

    @Input() iconType: string = 'IMG'; // MD, TB, CLASS, IMG  
    @Input() icon: string = '';

    imgUrl: string;

    constructor(private httpService: HttpService) {
        this.imgUrl = this.httpService.getDocumentBaseUrl() + 'getImage/?src=';
    }

}
