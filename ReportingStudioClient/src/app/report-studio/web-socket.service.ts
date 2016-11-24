import { Subject, Observable, Observer } from 'rxjs/Rx';
import { Injectable } from '@angular/core';
import 'rxjs/add/observable/of';

@Injectable()
export class WebSocketService {

  private ws: WebSocket;

  private subject: Subject<MessageEvent>;

  /**
   * WS Ready state constants
   *
   * 0 => CONNECTING
   *
   * 1 => OPEN
   *
   * 2 => CLOSING
   *
   * 3 => CLOSED
   */
  wsConnectionState$: Observable<number>;
  wsConnectionStateObserver: Observer<number>;

  public connect(url): Subject<MessageEvent> {
    if (!this.subject) {
      this.subject = this.create(url);
    }

    return this.subject;
  }

  private create(url): Subject<MessageEvent> {
    this.ws = new WebSocket(url);

    this.wsConnectionState$ = new Observable<number>(
      observer => {
        this.wsConnectionStateObserver = observer;
      }
    ).share();

    let observable = Observable.create((obs: Observer<MessageEvent>) => {
      this.ws.onmessage = obs.next.bind(obs);
      this.ws.onerror = obs.error.bind(obs);
      this.ws.onclose = obs.complete.bind(obs);

      return this.ws.close.bind(this.ws);
    });

    let observer = {
      next: (data: Object) => {
        if (this.ws.readyState === WebSocket.OPEN) {
          this.ws.send(JSON.stringify(data));
        }
      },
    };

    return Subject.create(observer, observable);
  }
}
