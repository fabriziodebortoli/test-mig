import { Injectable } from '@angular/core';

import { Logger } from './logger.service';

@Injectable()
export class UrlService {

    private hostname: string;
    private port: number = 5000;
    private secure: boolean = false;

    constructor(private logger: Logger) { }

    getBackendUrl() {
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
        if (!this.hostname) {
            this.hostname = window.location.hostname;
        }
        let protocol = 'ws:';
        if (this.secure) {
            protocol = 'wss:';
        }
        return protocol += '//' + this.hostname + ':' + this.port + '/tbloader';
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