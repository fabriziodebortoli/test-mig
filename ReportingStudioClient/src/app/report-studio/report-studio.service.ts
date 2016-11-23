import { WebSocketService } from './web-socket.service';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Subject } from 'rxjs';

const WS_URL = 'ws://localhost:5000';
const SERVER_URL = 'http://localhost:5000';

export interface Message {
  command: Command;
  message: string;
  response?: string;
}

export enum Command {
  DATA,
  STRUCT,
  ASK,
  TEST,
  GUID,
  ERROR,
  PAGE,
  PDF
}

@Injectable()
export class ReportStudioService {

  private socket: Subject<MessageEvent>;
  public messages: Subject<Message>;

  public wsConnectionState: Observable<number>;

  constructor(
    private http: Http,
    private websocketService: WebSocketService) { }

  connect() {
    this.socket = this.websocketService.connect(WS_URL);

    // this.wsConnectionState = <Subject<number>>this.websocketService.getConnectionState();
    this.wsConnectionState = this.websocketService.wsConnectionState;
    // this.websocketService.getConnectionState().subscribe((readyState: number) => {
    //   this.wsConnectionState.next(readyState);
    // });

    this.messages = <Subject<Message>>this.socket
      .map((response: MessageEvent): Message => {
        let data = JSON.parse(response.data);
        return data;
      });
  }

  runReport(namespace: string): Observable<any> {
    return this.http
      .get(SERVER_URL + `/api/RSWeb/${namespace}`)
      .map((r: Response) => r.json())
      .catch(this.handleError);
  }

  sendTestMessage(message) {
    this.messages.next(message);
    console.log(message)
  }

  private handleError(error: any): Observable<any> {
    console.error('An error occurred', error);
    return Observable.onErrorResumeNext(error.message || error);
  }

}
