import { HttpService } from './http.service';
import { Injectable } from '@angular/core';

@Injectable()
export class InfoService {
    desktop: boolean;
    constructor(httpService: HttpService) {
        // let subs = httpService.getInstallationInfo().subscribe(info => {
        //     this.desktop = info.desktop;
        //     subs.unsubscribe();
        // });

    }



}