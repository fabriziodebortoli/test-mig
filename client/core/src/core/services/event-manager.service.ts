import { Injectable, EventEmitter } from '@angular/core';
import { Logger } from './../../core/services/logger.service';

@Injectable()
export class EventManagerService {

    preferenceLoaded: EventEmitter<any> = new EventEmitter();
    loggedIn: EventEmitter<any> = new EventEmitter();
    loggingOff: EventEmitter<any> = new EventEmitter();

    constructor(public logger: Logger) { }

    emitPreferenceLoaded() {
        this.preferenceLoaded.emit();
    }

    emitLoggedIn() {
        this.loggedIn.emit();
    }

    emitloggingOff() {
        this.loggingOff.emit();
    }

}