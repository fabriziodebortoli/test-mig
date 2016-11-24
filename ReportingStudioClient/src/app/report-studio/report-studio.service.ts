import { WebSocketService } from './web-socket.service';
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Subject } from 'rxjs';

const WS_URL = 'ws://localhost:5000';
const SERVER_URL = 'http://localhost:5000';

export interface Message {
  commandType: CommandType;
  message: string;
  response?: string;
}

export enum CommandType {
  OK,
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
  public messages: Subject<Message> = new Subject<Message>();

  public wsConnectionState$: Observable<number> = Observable.of(WebSocket.CLOSED);

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

  runReport(namespace: string): Observable<Message> {
    return this.http
      .get(SERVER_URL + `/api/RSWeb/${namespace}`)

      .map((r: Response) => <Message>r.json())
      .catch(this.handleError);
  }

  sendGUID(guid: string) {
    let m: Message = {
      commandType: CommandType.GUID,
      message: guid
    };
    this.send(m);
  }

  sendTestMessage(text: string) {
    let m: Message = {
      commandType: CommandType.TEST,
      message: text
    };
    this.send(m);
  }

  send(message: Message) {
    if (this.websocketService.getConnectionState() === WebSocket.OPEN) {
      this.messages.next(message);
    } else {
      console.error('WebSocket disconnected - Cannot send message');
    }
  }

  private handleError(error: any): Observable<any> {
    console.log('An error occurred:', error);
    return Observable.throw(error || 'An error occurred');
  }

}
