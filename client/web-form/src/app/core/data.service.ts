import { environment } from './../../environments/environment';
import { Observable } from 'rxjs/Rx';
import { URLSearchParams, Http, Response } from '@angular/http';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from 'libclient';
import { Injectable } from '@angular/core';


@Injectable()
export class DataService extends DocumentService {

  constructor(logger: Logger, eventData: EventDataService, private http: Http) {
    super(logger, eventData);
  }

  getData(nameSpace: string, params: URLSearchParams): Observable<any> {

    let url: string = environment.baseUrl + 'data-service/getdata/' + nameSpace;

    return this.http.get(url, { search: params }).map((res: Response) => res.text());
  }

  getSelections(nameSpace: string): Observable<any> {
    let url: string = environment.baseUrl + 'data-service/getselections/' + nameSpace;

    return this.http.get(url).map((res: Response) => res.text());
  }

}
