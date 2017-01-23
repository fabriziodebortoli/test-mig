import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';

import { Logger } from 'libclient';

@Injectable()
export class DocumentService {
    model: any;
    serverSideCommandMap: any; //TODO SILVANO needs typing  
    mainCmpId: string;
    dataReadySubscription: any;
    serverCommandMapReadySubscription: any;
    constructor(private webSocketService: WebSocketService,
        private logger: Logger) {
        this.dataReadySubscription = this.webSocketService.dataReady.subscribe(data => {
            let models: Array<any> = data.models;
            let cmpId = this.mainCmpId;
            models.forEach(model => {
                if (model.id === cmpId) {
                    this.model = model;
                    logger.debug("Model received from server: " + JSON.stringify(this.model));
                }
            });
        });

        this.serverCommandMapReadySubscription = this.webSocketService.serverCommandMapReady.subscribe(data => {
            let cmpId = this.mainCmpId;
               if (data.id === cmpId) {
                    this.serverSideCommandMap = data.map
                    logger.debug("Server-side commands received from server: " + JSON.stringify(this.serverSideCommandMap));
                }
            });

    }

    dispose()
    {
        delete this.model;
        delete this.serverSideCommandMap;
        delete this.mainCmpId;
        this.dataReadySubscription.unsubscribe();
        this.serverCommandMapReadySubscription.unsubscribe();
    }
}
