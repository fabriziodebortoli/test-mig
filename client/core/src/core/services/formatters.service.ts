import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { HttpService } from './http.service';

@Injectable()
export class FormattersService {

    public formattersTable: any;
    constructor(public httpService: HttpService) { }

    loadFormattersTable() {
        this.httpService.getFormattersTable().subscribe((json) => {
            this.formattersTable = json.formatters;
        });
    }

    async loadFormattersTableAsync() {
        if (!this.formattersTable) {
            let result = await this.httpService.getFormattersTable().toPromise();
            this.formattersTable = result.formatters;
        }
    }

    getFormatter(key: string): any {
        return this.formattersTable[key];
    }

}
