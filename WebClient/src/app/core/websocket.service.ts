import { Injectable } from '@angular/core';
import 'rxjs/add/operator/toPromise';
import { HttpService } from './http.service';
import { Logger } from 'libclient';
import { CookieService } from 'angular2-cookie/services/cookies.service';

@Injectable()
export class WebSocketService {
    public status: string = 'Undefined';
    private events: Object = {};
    private connection: WebSocket;

    constructor(private httpService: HttpService,
        private cookieService: CookieService,
        private logger: Logger) {
    }

    wsConnect(): void {
        let $this = this;
        this.httpService.getWebSocketPort()
            .subscribe(port => {
                let url = 'ws://' + window.location.hostname + ':' + port + '/TBWebSocketsController';
                this.logger.debug('wsConnecting... ' + url);

                this.connection = new WebSocket(url);
                this.connection.onmessage = function (e) {
                    if (typeof (e.data) === 'string') {
                        let idx = e.data.indexOf('-');
                        let eventName = e.data.substr(0, idx);
                        let jsonString = e.data.substr(idx + 1);
                        let jsonObject = null;
                        if (jsonString !== '') {
                            try {
                                jsonObject = JSON.parse(jsonString);
                                $this.logger.debug('wsOnMessage', eventName, jsonObject);
                            } catch (e) {
                                $this.logger.error('wsOnMessage', eventName, e);
                            }
                        }
                        $this.fire(eventName, jsonObject);
                    }
                };
                this.connection.onerror = (arg) => {
                    this.logger.error('wsOnError' + JSON.stringify(arg));
                    this.fire('error', arg);
                    this.status = 'Error';
                };

                this.connection.onopen = (arg) => {
                    this.logger.debug('wsOnOpen');
                    this.connection.send('SetWebSocketName:' + this.cookieService.get('authtoken'));
                    this.status = 'Open';
                    this.fire('open', arg);
                };

                this.connection.onclose = (arg) => {
                    this.logger.debug('wsOnClose');
                    this.fire('close', arg);
                    this.status = 'Closed';
                };
            });
    }

    on(evtName, fn) {
        let evt = this.events[evtName];
        if (!evt) {
            evt = [];
            this.events[evtName] = evt;
        }
        evt.push(fn);
    }


    fire(evtName, data) {
        let evt = this.events[evtName];
        if (evt) {
            for (let i = 0; i < evt.length; i++) {
                evt[i](data);
            }
        }
    }

    wsClose() {
        if (this.connection) {
            this.connection.close();
        }
    }
}
