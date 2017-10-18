import { Injectable, EventEmitter } from '@angular/core';
import { Observable, Observer } from 'rxjs/Rx';

import { Logger } from './../../core/services/logger.service';

@Injectable()
export class EventManagerService {

    preferenceLoaded: EventEmitter<any> = new EventEmitter();
    loggedIn: EventEmitter<any> = new EventEmitter();
    loggedOff: EventEmitter<any> = new EventEmitter();
    loggingOff: EventEmitter<any> = new EventEmitter();

    constructor(public logger: Logger) {
        this.logger.debug('EventManagerService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    emitPreferenceLoaded() {
        this.preferenceLoaded.emit();
    }

    emitLoggedIn() {
        this.loggedIn.emit();
    }

    emitloggedOff() {
        this.loggedOff.emit();
    }

    emitloggingOff() {
        this.loggingOff.emit();
    }

}