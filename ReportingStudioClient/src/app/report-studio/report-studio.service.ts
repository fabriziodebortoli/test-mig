import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs';

@Injectable()
export class ReportStudioService {

  private server: string = 'http://localhost:5000';
  private apiRunReport: string = 'api/report';

  constructor(private http: Http) { }

  runReport(namespace: string): Observable<any> {

    return this.http
      .get(`${this.server}/${this.apiRunReport}/${namespace}`)
      .map((r: Response) => r.json());
  }

  runReportTest(namespace: string): Observable<any> {


    return this.http
      .get('http://localhost:4200/runreport.json')
      .map((r: Response) => r.json());
  }

}
