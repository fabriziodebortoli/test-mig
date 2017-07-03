import { Injectable, EventEmitter } from '@angular/core';
import { Observable, Observer } from 'rxjs/Rx';

import { Logger } from './../../core/services/logger.service';

@Injectable()
export class EventManagerService {

    preferenceLoaded: EventEmitter<any> = new EventEmitter();

    constructor(private logger: Logger) {
        this.logger.debug('EventManagerService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    emitPreferenceLoaded() {
        this.preferenceLoaded.emit();
    }

}