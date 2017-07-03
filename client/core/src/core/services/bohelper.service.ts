import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';

@Injectable()
export class BOHelperService {
    constructor(
        public logger: Logger,
        private webSocketService: WebSocketService) {
    }
}
