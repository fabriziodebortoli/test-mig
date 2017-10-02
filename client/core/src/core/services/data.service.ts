import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response, RequestOptions, RequestOptionsArgs } from '@angular/http';

import { Observable } from 'rxjs/Rx';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { InfoService } from './info.service';
import { Logger } from './logger.service';

@Injectable()
export class DataService extends DocumentService {

  constructor(
    public logger: Logger,
    public eventData: EventDataService,
    public http: Http,
    public infoService: InfoService
  ) {
    super(logger, eventData, infoService);
  }

  getData(nameSpace: string, selectionType: string, params: URLSearchParams): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getdata/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { search: params, withCredentials: true }).map((res: Response) => res.json());
  }

  getColumns(nameSpace: string, selectionType: string): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getcolumns/' + nameSpace + '/' + selectionType;

    return this.http.get(url, { withCredentials: true }).map((res: Response) => res.json());
  }

  getSelections(nameSpace: string): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getselections/' + nameSpace;

    return this.http.get(url, { withCredentials: true }).map((res: Response) => res.json());
  }

  getParameters(nameSpace: string): Observable<Response> {
    let url: string = this.infoService.getBaseUrl() + '/data-service/getparameters/' + nameSpace;

    return this.http.get(url, { withCredentials: true }).map((res: Response) => res.json());
  }

  getRadarData(params: URLSearchParams) {
    let url: string = this.infoService.getBaseUrl() + '/data-service/radar';// /' + params.get('query');
    let options = { withCredentials: true };

    return this.http.post(url, params, options).map((res: Response) => res.json());
  }

}
