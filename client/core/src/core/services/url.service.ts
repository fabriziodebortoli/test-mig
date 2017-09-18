import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { Injectable } from '@angular/core';

import { Logger } from './logger.service';

@Injectable()
export class UrlService {

    private hostname: string;
    private port: number = 5000;
    private secure: boolean = false;
    private baseUrl: string = "";
    private wsBaseUrl: string = "";
    constructor(private logger: Logger, private http: Http) {

        // this.getConfiguration()
        //     .then((res) => {
        //         let js = res.json();
        //         this.baseUrl = js.baseUrl;
        //         this.wsBaseUrl = js.wsBaseUrl;
        //     })
        //     .catch(
        //     err => {
        //         console.log(err);
        //     })

    }

    async getConfiguration(): Promise<any> {
        return await this.http.get('assets/config.json').toPromise();
    }

    init() {
        return this.http.get('assets/config.json')
            .map((res) => {
                let js = res.json();
                this.baseUrl = js['baseUrl'];
                this.wsBaseUrl = js['wsBaseUrl'];
            })
        //.catch(err => console.log(err));
    }

    getBackendUrl() {
        if (this.baseUrl)
            return this.baseUrl;

        if (!this.hostname) {
            this.hostname = window.location.hostname;
        }

        let protocol = 'http:';
        if (this.secure) {
            protocol = 'https:';
        }
        return protocol += '//' + this.hostname + ':' + this.port;
    }

    getApiUrl() {
        return this.getBackendUrl() + '/tbloader/api/';
    }

    getWsUrl() {
        return this.getWsBaseUrl() + '/tbloader';
    }

    getWsBaseUrl() {
        if (this.wsBaseUrl)
            return this.wsBaseUrl;
        if (!this.hostname) {
            this.hostname = window.location.hostname;
        }
        let protocol = 'ws:';
        if (this.secure) {
            protocol = 'wss:';
        }
        return protocol += '//' + this.hostname + ':' + this.port;
    }

    setPort(port: number) {
        this.port = port;
    }

    setHostname(hostname: string) {
        this.hostname = hostname;
    }

    setSecure(secure: boolean) {
        this.secure = secure;
    }
}