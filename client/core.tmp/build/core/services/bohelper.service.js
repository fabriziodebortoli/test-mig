import { Injectable } from '@angular/core';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';
export class BOHelperService {
    /**
     * @param {?} logger
     * @param {?} webSocketService
     */
    constructor(logger, webSocketService) {
        this.logger = logger;
        this.webSocketService = webSocketService;
    }
}
BOHelperService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
BOHelperService.ctorParameters = () => [
    { type: Logger, },
    { type: WebSocketService, },
];
function BOHelperService_tsickle_Closure_declarations() {
    /** @type {?} */
    BOHelperService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    BOHelperService.ctorParameters;
    /** @type {?} */
    BOHelperService.prototype.logger;
    /** @type {?} */
    BOHelperService.prototype.webSocketService;
}
