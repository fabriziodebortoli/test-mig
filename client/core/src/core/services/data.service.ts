import { TbComponentServiceParams } from './tbcomponent.service.params';
import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response, RequestOptions, RequestOptionsArgs, Headers } from '@angular/http';
import { Observable } from '../../rxjs.imports';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';

@Injectable()
export class DataService extends DocumentService {

  constructor(
    params: TbComponentServiceParams,
    eventData:EventDataService,
    public http: Http
  ) {
    super(params, eventData);
  }

  // TODO refactor auth headers
  createAuthorizationHeader() {
    let headers = new Headers();
    headers.append("Authorization", this.infoService.getAuthorization());
    return headers;
  }

  getData(nameSpace: string, selectionType: string, params: URLSearchParams): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getdata/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { headers: this.createAuthorizationHeader(), search: params, withCredentials: true }).map((res: Response) => res.json());
  }

  getColumns(nameSpace: string, selectionType: string): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getcolumns/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { headers: this.createAuthorizationHeader(), withCredentials: true }).map((res: Response) => res.json());
  }

  getSelections(nameSpace: string): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getselections/' + nameSpace;

    return this.http.get(url, { headers: this.createAuthorizationHeader(), withCredentials: true }).map((res: Response) => res.json());
  }

  getParameters(nameSpace: string): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getparameters/' + nameSpace;

    return this.http.get(url, { headers: this.createAuthorizationHeader(), withCredentials: true }).map((res: Response) => res.json());
  }

    getRadarData(params: URLSearchParams /*nameSpace: string, type: string*/) {
        let url: string = this.infoService.getBaseUrl() + '/data-service/radar/'/*/nameSpace/type*/;// /' + params.get('query');
    let options = { headers: this.createAuthorizationHeader(), withCredentials: true };

    return this.http.get(url, { headers: this.createAuthorizationHeader(), search: params, withCredentials: true }).map((res: Response) => res.json());
  }

}
