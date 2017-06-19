import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';
export declare class BOHelperService {
    logger: Logger;
    private webSocketService;
    constructor(logger: Logger, webSocketService: WebSocketService);
}
