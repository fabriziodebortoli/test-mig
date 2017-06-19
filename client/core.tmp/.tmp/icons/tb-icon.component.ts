import { Component, Input } from '@angular/core';
// import { HttpService } from './../../core/http.service';

@Component({
    selector: 'tb-icon',
    template: "<div [ngSwitch]=\"iconType\" class=\"div-icon\"> <img *ngSwitchCase=\"'IMG'\" src=\"{{imgUrl}}{{icon}}\" /> <m4-icon *ngSwitchCase=\"'M4'\" icon=\"{{icon}}\"></m4-icon> <i *ngSwitchCase=\"'CLASS'\" class=\"{{icon}}\">asdgf</i> <h5 *ngSwitchDefault>no-icon</h5> </div>"
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
