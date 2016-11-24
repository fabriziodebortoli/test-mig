import { WebSocketService } from './web-socket.service';
import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable, Subject } from 'rxjs';

const WS_URL = 'ws://localhost:5000';

export interface Message {
  commandType: CommandType;
  message: string;
  response?: string;
}

export enum CommandType { OK, NAMESPACE, DATA, STRUCT, ASK, TEST, GUID, ERROR, PAGE, PDF }

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
