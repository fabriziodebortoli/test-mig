import { Injectable } from '@angular/core';

@Injectable()
export class UrlService {

    private host: string;
    private port: number = 5000;
    private secure: boolean = false;

    constructor() { }

    getBackendUrl() {
        if (!this.host) {
            this.host = window.location.host;
        }
        let protocol = 'http:';
        if (this.secure) {
            protocol = 'https:';
        }
        return protocol += '//' + this.host + ':' + this.port;
    }

    getApiUrl() {
        return this.getBackendUrl() + '/tbloader/api/';
    }

    getWsUrl() {
        if (!this.host) {
            this.host = window.location.host;
        }
        let protocol = 'ws:';
        if (this.secure) {
            protocol = 'wss:';
        }
        return protocol += '//' + this.host + ':' + this.port;
    }

    setPort(port: number) {
        this.port = port;
    }

    setHost(host: string) {
        this.host = host;
    }

    setSecure(secure: boolean) {
        this.secure = secure;
    }
}