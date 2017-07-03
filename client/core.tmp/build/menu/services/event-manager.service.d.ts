import { EventEmitter } from '@angular/core';
import { Logger } from '../../core/services/logger.service';
export declare class EventManagerService {
    private logger;
    preferenceLoaded: EventEmitter<any>;
    constructor(logger: Logger);
    emitPreferenceLoaded(): void;
}
