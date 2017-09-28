import { environment } from 'environments/environment';
import { Injectable } from '@angular/core';

@Injectable()
export class Logger {

    loggerDebug: boolean; // TODO leggere da configurazione esterna

    constructor() {
        this.loggerDebug = environment.loggerDebug;
        this.debug('Logger service init', this.loggerDebug);

    }

    log(message?: any, ...optionalParams: any[]): void {
        console.log(message, ...optionalParams);
    }

    info(message?: any, ...optionalParams: any[]): void {
        console.info(message, ...optionalParams);
    }

    debug(message?: any, ...optionalParams: any[]): void {
        if (this.loggerDebug) {
            console.log(message, ...optionalParams);
        }
    }

    warn(message?: any, ...optionalParams: any[]): void {
        console.warn(message, ...optionalParams);
    }

    error(message?: any, ...optionalParams: any[]): void {
        console.error(message, ...optionalParams);
    }
}
