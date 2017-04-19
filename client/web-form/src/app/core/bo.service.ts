import { Injectable } from '@angular/core';

import { Logger } from 'libclient';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { apply } from 'json8-patch';

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap: any; //TODO SILVANO needs typing  

    //subscriptions
    dataReadySubscription: any;
    activationDataSubscription: any;
    serverCommandMapReadySubscription: any;
    commandSubscription: any;
    changeSubscription: any;
    openDropdownSubscription: any;

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
                    if (this.eventData.model) {
                        this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
                    }
                    if (model.patch) {
                        const patched = apply({ 'data': this.eventData.model }, model.patch);
                        model.data = patched.doc.data;
                    }
                    if (model.data) {
                        for (let prop in model.data) {
                            if (model.data.hasOwnProperty(prop)) {
                                this.eventData.model[prop] = model.data[prop];
                            }
                        }
                    }
                }
            });
        });

        this.activationDataSubscription = this.webSocketService.activationData.subscribe(data => {
            let components: Array<any> = data.components;
            let cmpId = this.mainCmpId;
            components.forEach(cmp => {
                if (cmp.id === cmpId) {
                    this.eventData.model._activation = cmp.activation;
                }
            });
        });

        this.serverCommandMapReadySubscription = this.webSocketService.serverCommandMapReady.subscribe(data => {
            let cmpId = this.mainCmpId;
            if (data.id === cmpId) {
                this.serverSideCommandMap = data.map
            }
        });
        this.commandSubscription = this.eventData.command.subscribe((cmpId: String) => {
            this.webSocketService.doCommand(this.mainCmpId, cmpId, this.eventData.model);
        });

        this.changeSubscription = this.eventData.command.subscribe((cmpId: String) => {
            this.webSocketService.doValueChanged(this.mainCmpId, cmpId, this.eventData.model);
        });

        this.openDropdownSubscription = this.eventData.openDropdown.subscribe((obj: any) => {
            this.webSocketService.doFillListBox(this.mainCmpId, obj);
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
        this.activationDataSubscription.unsubscribe();
        this.openDropdownSubscription.unsubscribe();
    }

    close() {
        super.close();
        this.webSocketService.doCommand(this.mainCmpId, 'ID_FILE_CLOSE');
    }
}
