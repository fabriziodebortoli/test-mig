import { environment } from './../../environments/environment';
import { Observable } from 'rxjs/Rx';
import { URLSearchParams, Http, Response } from '@angular/http';

import { Injectable } from '@angular/core';

@Injectable()
export class DataService {

  constructor(private http: Http) { }

  getData(nameSpace: string, selectionType: string, params: URLSearchParams) {

    const url: string = environment.baseUrl + 'data-service/getdata/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { search: params }).map((res: Response) => res.json());
  }

  getColumns(nameSpace: string, selectionType: string) {

    const url: string = environment.baseUrl + 'data-service/getcolumns/' + nameSpace + '/' + selectionType;

    return this.http.get(url).map((res: Response) => res.json());
  }

  getSelections(nameSpace: string) {
    const url: string = environment.baseUrl + 'data-service/getselections/' + nameSpace;

    return this.http.get(url).map((res: Response) => res.json());
  }

  getParameters(nameSpace: string) {
    const url: string = environment.baseUrl + 'data-service/getparameters/' + nameSpace;

    return this.http.get(url).map((res: Response) => res.json());
  }

}
