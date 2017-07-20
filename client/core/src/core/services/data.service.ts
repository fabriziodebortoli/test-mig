import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response, RequestOptions, RequestOptionsArgs } from '@angular/http';

import { Observable } from 'rxjs/Rx';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';

@Injectable()
export class DataService extends DocumentService {

  constructor(logger: Logger, eventData: EventDataService, private http: Http, private urlService: UrlService) {
    super(logger, eventData);
  }

  getData(nameSpace: string, selectionType: string, params: URLSearchParams): Observable<Response> {
    let url: string = this.urlService.getBackendUrl() + '/data-service/getdata/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { search: params, withCredentials: true }).map((res: Response) => res.json());
  }

  getColumns(nameSpace: string, selectionType: string): Observable<Response> {
    let url: string = this.urlService.getBackendUrl() + '/data-service/getcolumns/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { withCredentials: true }).map((res: Response) => res.json());
  }

  getSelections(nameSpace: string): Observable<Response> {
    let url: string = this.urlService.getBackendUrl() + '/data-service/getselections/' + nameSpace;

    return this.http.get(url, { withCredentials: true }).map((res: Response) => res.json());
  }

  getParameters(nameSpace: string): Observable<Response> {
    let url: string = this.urlService.getBackendUrl() + '/data-service/getparameters/' + nameSpace;

    return this.http.get(url, { withCredentials: true }).map((res: Response) => res.json());
  }

  getRadarData(params: URLSearchParams) {
    let url: string = this.urlService.getBackendUrl() + '/data-service/radar';// /' + params.get('query');
    let options = { withCredentials: true };

    return this.http.post(url, params, options).map((res: Response) => res.json());
  }

}
