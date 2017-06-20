import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';
import { Logger } from '@taskbuilder/core';

@Injectable()
export class BOHelperService {
    constructor(
        public logger: Logger,
        private webSocketService: WebSocketService) {
    }
}

