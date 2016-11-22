import { WebSocketService } from './web-socket.service';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Subject } from 'rxjs';

const WS_URL = 'ws://localhost:5000';
const SERVER_URL = 'http://localhost:5000';

export interface Message {
  command: Command;
  message: string;
}

export enum Command {
  DATA,
  STRUCT,
  ASK,
  TEST
}

@Injectable()
export class ReportStudioService {

  private socket: Subject<MessageEvent>;
  public messages: Subject<Message>;

  constructor(
    private http: Http,
    private websocketService: WebSocketService) { }

  connect() {
    this.socket = this.websocketService.connect(WS_URL);

    this.messages = <Subject<Message>>this.socket
      .map((response: MessageEvent): Message => {
        let data = JSON.parse(response.data);
        return data;
      });
  }

  runReport(namespace: string): Observable<any> {
    return this.http
      .get(SERVER_URL + `/api/report/${namespace}`)
      .map((r: Response) => r.json());
  }

  runReportTest(namespace: string): Observable<any> {
    return this.http
      .get('http://localhost:4200/runreport.json')
      .map((r: Response) => r.json());
  }

}
