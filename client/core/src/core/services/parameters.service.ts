import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
import { InfoService } from './info.service';

// import { Observable } from 'rxjs.imports';

@Injectable()
export class ParameterService {
    private _parameters = [];
    constructor(private http: HttpService, private infoService: InfoService) { }

    async getParameter(param: string): Promise<string> {
        const _p = param.split('.');
        if (_p.length !== 2) { return null; }

        const tableName = _p[0];
        const columnName = _p[1];

        if (this._parameters[tableName]) {
            const value = this._parameters[tableName][columnName];
            return value ? value : null;
        }

        this._parameters[tableName] = await this.updateCachedParamTable(tableName);
        return this._parameters[tableName][columnName];
    }

    private async updateCachedParamTable(table): Promise<any> {
        const url = this.infoService.getBaseUrl() + '/data-service/parameters/getparameters';
        const r = await this.http.postData(url, { table: table }).toPromise();
        return r.json();
    }
}
