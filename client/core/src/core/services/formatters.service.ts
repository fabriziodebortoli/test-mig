import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { HttpService } from './http.service';

@Injectable()
export class FormattersService {

    public formattersTable: any;
    constructor(public httpService: HttpService) { }

    getFormattersTable() {
        this.httpService.getFormattersTable().subscribe((json) => {
            this.formattersTable = json.enums;
        });
    }

    async getFormattersTableAsync() {
        if (!this.formattersTable) {
            let result = await this.httpService.getFormattersTable().toPromise();
            this.formattersTable = result.enums;
        }
    }

}
