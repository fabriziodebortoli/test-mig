import { Component, Input } from '@angular/core';
// import { HttpService } from './../../core/http.service';

@Component({
    selector: 'tb-icon',
    templateUrl: './tb-icon.component.html'
})
export class TbIconComponent {

    @Input() iconType: string = 'IMG'; // TB, CLASS, IMG  
    @Input() icon: string = '';

    imgUrl: string;

    // TODO import core http services
    // constructor(private httpService: HttpService) {
    //     this.imgUrl = this.httpService.getDocumentBaseUrl() + 'getImage/?src=';
    // }

}
