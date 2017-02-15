import { EventService } from 'tb-core';
import { DocumentService } from './document.service';
import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';

import { Logger } from 'libclient';

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap: any; //TODO SILVANO needs typing  

    //subscriptions
    dataReadySubscription: any;
    serverCommandMapReadySubscription: any;
    commandSubscription: any;
    changeSubscription: any;

    constructor(
        private webSocketService: WebSocketService,
        logger: Logger,
        eventService: EventService) {
        super(logger, eventService);

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
        this.commandSubscription = this.eventService.command.subscribe((cmpId: String) => {
            this.webSocketService.doCommand(this.mainCmpId, cmpId, this.model);
        });

        this.changeSubscription = this.eventService.command.subscribe((cmpId: String) => {
            this.webSocketService.doValueChanged(this.mainCmpId, cmpId, this.model);
        });
    }
    init(cmpId: string) {
        super.init(cmpId);
        this.webSocketService.getDocumentData(this.mainCmpId);
    }
    dispose() {
        super.dispose();
        delete this.serverSideCommandMap;
        this.dataReadySubscription.unsubscribe();
        this.serverCommandMapReadySubscription.unsubscribe();
        this.commandSubscription.unsubscribe();
        this.changeSubscription.unsubscribe();
    }
}
