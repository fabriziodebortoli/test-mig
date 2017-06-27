import { URLSearchParams, Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { UrlService } from './url.service';
import { Logger } from './logger.service';
export declare class DataService extends DocumentService {
    private http;
    private urlService;
    constructor(logger: Logger, eventData: EventDataService, http: Http, urlService: UrlService);
    getData(nameSpace: string, selectionType: string, params: URLSearchParams): Observable<Response>;
    getColumns(nameSpace: string, selectionType: string): Observable<Response>;
    getSelections(nameSpace: string): Observable<Response>;
    getParameters(nameSpace: string): Observable<Response>;
}
