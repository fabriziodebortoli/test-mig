import { Injectable, EventEmitter } from '@angular/core';
import 'rxjs/add/operator/toPromise';
import { HttpService } from './http.service';
import { Logger } from 'libclient';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class WebSocketService {
    public status: string = 'Undefined';
    private connection: WebSocket;

    public error: EventEmitter<any> = new EventEmitter();
    public dataReady: EventEmitter<any> = new EventEmitter();
    public windowOpen: EventEmitter<any> = new EventEmitter();
    public windowClose: EventEmitter<any> = new EventEmitter();
    public open: EventEmitter<any> = new EventEmitter();
    public close: EventEmitter<any> = new EventEmitter();

    constructor(private httpService: HttpService,
        private cookieService: CookieService,
        private logger: Logger) {
    }

    wsConnect(): void {
        let $this = this;

        let url = 'ws://' + window.location.hostname + ':' + this.httpService.gatePort;
        this.logger.debug('wsConnecting... ' + url);

        this.connection = new WebSocket(url);
        this.connection.onmessage = function (e) {
            if (typeof (e.data) === 'string') {
                let idx = e.data.indexOf('-');
                let eventName = e.data.substr(0, idx);
                let jsonString = e.data.substr(idx + 1);
                if (jsonString !== '') {
                    try {
                        let jsonObject = JSON.parse(jsonString);
                        $this.logger.debug('wsOnMessage', eventName, jsonObject);
                        switch (eventName) {
                            case 'DataReady': $this.dataReady.emit(jsonObject); break;
                            case 'WindowOpen': $this.windowOpen.emit(jsonObject); break;
                            case 'WindowClose': $this.windowClose.emit(jsonObject); break;
                            default: break;
                        }

                    } catch (e) {
                        $this.logger.error('Invalid json string:\n' + jsonString);
                        $this.logger.error('wsOnMessage', eventName, e);
                    }
                }

            }
        };
        this.connection.onerror = (arg) => {
            this.logger.error('wsOnError' + JSON.stringify(arg));
            this.error.emit(arg);
            this.status = 'Error';
        };

        this.connection.onopen = (arg) => {
            this.logger.debug('wsOnOpen');
            //sets the name for this client socket 
            this.connection.send('SetClientWebSocketName:' + this.cookieService.get('authtoken'));
            //stimulate tbloader to open a client connection with the same name, so che gate can pass-through
            this.httpService.openServerSocket(this.cookieService.get('authtoken'));
            this.status = 'Open';
            this.open.emit(arg);
        };

        this.connection.onclose = (arg) => {
            this.logger.debug('wsOnClose');
            this.close.emit(arg);
            this.status = 'Closed';
        };
    }



    wsClose() {
        if (this.connection) {
            this.connection.close();
        }
    }
}
export class SocketMessage {
    constructor(public name: string, public content: any) {

    }
}