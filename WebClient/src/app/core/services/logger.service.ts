import { Injectable } from '@angular/core';
import { Http } from '@angular/http';

import { UtilsService } from './utils.service';

@Injectable()
export class Logger {

    constructor() {
        console.log('LoggerService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    log(message?: any, ...optionalParams: any[]): void {
        console.log(message, ...optionalParams);
    }

    info(message?: any, ...optionalParams: any[]): void {
        console.log(message, ...optionalParams);
    }

    debug(message?: any, ...optionalParams: any[]): void {
        console.debug(message, ...optionalParams);
    }

    warn(message?: any, ...optionalParams: any[]): void {
        console.warn(message, ...optionalParams);
    }

    error(message?: any, ...optionalParams: any[]): void {
        console.error(message, ...optionalParams);
    }
}
