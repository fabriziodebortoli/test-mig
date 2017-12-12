import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
import { InfoService } from './info.service';

@Injectable()
export class ParameterService {
    private _parameters = [];

    constructor(private http: HttpService, private infoService: InfoService)
    { }

    getParameters(params: string[]) {
        let result: { [key: string]: string };
        params.forEach(element => {
            result[element] = this.getParam(element);
        });
        return result;
    }

    private async getParam(param: string) {
        const _p = param.split('.');
        if (_p.length != 2) return 'unknow param';

        const tableName = _p[0];
        const columnName = _p[1];

        if (this._parameters[tableName]) {
            const value = this._parameters[tableName][columnName];
            return value ? value : 'unknown param';
        }

        await this.updateCachedParamTable(tableName);

        return await this.getParam(param);
    }

    private async updateCachedParamTable(table) {
        const url = this.infoService.getDocumentBaseUrl() + "parameters/getparameters"

        let r = await this.http.postData(url, table).toPromise();
        this._parameters[table] = r.json();
    }
}
