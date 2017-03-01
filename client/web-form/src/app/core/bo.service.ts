import { Injectable } from '@angular/core';

import { Logger } from 'libclient';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';


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
        eventData: EventDataService) {
        super(logger, eventData);

        this.dataReadySubscription = this.webSocketService.dataReady.subscribe(data => {
            let models: Array<any> = data.models;
            let cmpId = this.mainCmpId;
            models.forEach(model => {
                if (model.id === cmpId) {
                    this.eventData.model = model;
                    logger.debug("Model received from server: " + JSON.stringify(this.eventData.model));
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
        this.commandSubscription = this.eventData.command.subscribe((cmpId: String) => {
            this.webSocketService.doCommand(this.mainCmpId, cmpId, this.eventData.model);
        });

        this.changeSubscription = this.eventData.command.subscribe((cmpId: String) => {
            this.webSocketService.doValueChanged(this.mainCmpId, cmpId, this.eventData.model);
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
