import { Injectable } from '@angular/core';

@Injectable()
export class Logger {

    inDebug: boolean = true; // TODO leggere da configurazione esterna

    constructor() {
        this.debug('Logger service init');
    }

    log(message?: any, ...optionalParams: any[]): void {
        console.log(message, ...optionalParams);
    }

    info(message?: any, ...optionalParams: any[]): void {
        console.log(message, ...optionalParams);
    }

    debug(message?: any, ...optionalParams: any[]): void {
        if (this.inDebug) {
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
