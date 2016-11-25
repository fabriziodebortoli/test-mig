import { WebSocketService } from './web-socket.service';
import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable, Subject } from 'rxjs';

import { reportTest } from './test/report-test';

const WS_URL = 'ws://localhost:5000';

export interface Message {
  commandType: CommandType;
  message?: string;
  response?: string;
}

export enum CommandType { OK, NAMESPACE, DATA, STRUCT, ASK, TEST, GUID, ERROR, PAGE, PDF, RUN, PAUSE, STOP }

@Injectable()
export class ReportStudioService {

  private socket: Subject<MessageEvent>;

  public messages: Subject<Message> = new Subject<Message>();

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

  sendNamespace(ns: string) {
    let message: Message = {
      commandType: CommandType.NAMESPACE,
      message: ns
    };
    this.send(message);
  }

  sendRun() {
    let message: Message = {
      commandType: CommandType.RUN
    };
    this.send(message);
  }

  sendPause() {
    let message: Message = {
      commandType: CommandType.PAUSE
    };
    this.send(message);
  }

  sendStop() {
    let message: Message = {
      commandType: CommandType.STOP
    };
    this.send(message);
  }

  testSTRUCT() {
    let m: Message = {
      commandType: CommandType.STRUCT,
      message: JSON.stringify(reportTest)
    };
    this.send(m);
  }

  testDATA() {
    let m: Message = {
      commandType: CommandType.DATA,
      message: ''
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
      console.log('WebSocket, sent message', message);
    } else {
      console.error('WebSocket disconnected - Cannot send message');
    }
  }

  private handleError(error: any): Observable<any> {
    console.log('An error occurred:', error);
    return Observable.throw(error || 'An error occurred');
  }

}
