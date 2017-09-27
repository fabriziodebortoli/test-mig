import { Inject, Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Rx';

import { AppConfigModel } from './../../shared/models/app-config.model';

import { Logger } from './logger.service';

@Injectable()
export class AppConfigService {

    public config: AppConfigModel;

    constructor(private http: Http, private logger: Logger) { }

    public getConfig(key: any) {
        return this.config[key];
    }

    load() {
        return new Promise((resolve, reject) => {
            this.http.get('assets/config.json')
                .map(res => res.json())
                // .catch(this.httpService.handleError)
                .subscribe(config => {
                    this.config = config;
                    this.logger.debug("App Configuration", this.config)
                    resolve(true);
                });
        });
    }
}