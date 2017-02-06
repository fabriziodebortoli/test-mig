import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';

import { Logger } from 'libclient';

@Injectable()
export class DocumentService {
    model: any;
    serverSideCommandMap: string[];
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
    init(cmpId: string) {
        this.mainCmpId = cmpId;
        this.webSocketService.getDocumentData(this.mainCmpId);
    }
    getTitle() {
        let title = 'Untitled';
        if (this.model && this.model.Title && this.model.Title.value)
            title = this.model.Title.value;
        return title;
    }
    dispose() {
        delete this.model;
        delete this.serverSideCommandMap;
        delete this.mainCmpId;
        this.dataReadySubscription.unsubscribe();
        this.serverCommandMapReadySubscription.unsubscribe();
    }

    isServerSideCommand(idCommand: string) {
        return this.serverSideCommandMap.indexOf(idCommand) > -1;
    }
}
