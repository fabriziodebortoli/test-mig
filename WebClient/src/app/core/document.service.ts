import { WebSocketService } from './websocket.service';
import { Injectable } from '@angular/core';
@Injectable()
export class DocumentService {
    constructor(private webSocketService: WebSocketService) {

        this.webSocketService.on('DataReady', data => {
            console.debug(data);
        });
    }
}
