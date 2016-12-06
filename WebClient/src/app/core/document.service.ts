import { WebSocketService } from './websocket.service';
import { Injectable } from '@angular/core';
@Injectable()
export class DocumentService {
    data: any;

    constructor(private webSocketService: WebSocketService) {

        var me = this;
        this.webSocketService.on('DataReady', data => {
            console.debug(data);
            me.data = data
        });
    }
}
