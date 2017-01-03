import { WebSocketService } from './websocket.service';
import { Injectable } from '@angular/core';
import { Logger } from 'libclient';

@Injectable()
export class DocumentService {
    data: any;
    mainCmpId: string;
    constructor( private webSocketService: WebSocketService,
                 private logger: Logger) {
        this.webSocketService.dataReady.subscribe(data => {
            let models: Array<any> = data.models;
            let cmpId = this.mainCmpId;
            models.forEach(model => {
                if (model.id === cmpId) {
                    this.data = model;
                    logger.debug("Model received from server: " + JSON.stringify(this.data));
                }
            });

        });
    }
}
