import { AppConfigService } from './app-config.service';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import { Injectable } from '@angular/core';

import { Logger } from './logger.service';

@Injectable()
export class UrlService {

    private hostname: string;
    private port: number = 5000;
    private secure: boolean = false;

    constructor(private logger: Logger, private http: Http, private appConfigService: AppConfigService) { }

    getBackendUrl() {
        return this.appConfigService.config.baseUrl;
    }

    getApiUrl() {
        return this.getBackendUrl() + '/tbloader/api/';
    }

    getWsUrl() {
        return this.getWsBaseUrl() + '/tbloader';
    }

    getWsBaseUrl() {
        return this.appConfigService.config.wsBaseUrl;
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