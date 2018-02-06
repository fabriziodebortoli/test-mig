import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService, ParameterService } from '@taskbuilder/core';
export declare class ItemsHttpService {
    private httpService;
    private parametersService;
    controllerRoute: string;
    readonly DEFAULT_LEN_ITEM: number;
    constructor(httpService: HttpService, parametersService: ParameterService);
    getItemInfo_CodeLength(): Promise<number>;
    queryParam(param: string): Promise<string>;
    getItemsSearchList(queryType: string): Observable<Response>;
}
