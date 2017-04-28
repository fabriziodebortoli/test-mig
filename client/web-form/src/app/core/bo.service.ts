import { UtilsService } from './utils.service';
import { Injectable } from '@angular/core';

import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService, MessageDlgArgs } from './websocket.service';
import { apply, diff } from 'json8-patch';
import { BOHelperService } from "app/core/bohelper.service";

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap: any; //TODO SILVANO needs typing  

    //subscriptions
    messageSubscription: any;
    dataReadySubscription: any;
    activationDataSubscription: any;
    serverCommandMapReadySubscription: any;
    commandSubscription: any;
    changeSubscription: any;
    openDropdownSubscription: any;

    constructor(
        private webSocketService: WebSocketService,
        private boHelperService: BOHelperService,
        eventData: EventDataService) {
        super(boHelperService.logger, eventData);

        this.dataReadySubscription = this.webSocketService.dataReady.subscribe(data => {
            const models: Array<any> = data.models;
            const cmpId = this.mainCmpId;
            models.forEach(model => {
                if (model.id === cmpId) {
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
                    this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
                }
            });
        });

        this.activationDataSubscription = this.webSocketService.activationData.subscribe(data => {
            const components: Array<any> = data.components;
            const cmpId = this.mainCmpId;
            components.forEach(cmp => {
                if (cmp.id === cmpId) {
                    this.eventData.activation = cmp.activation;
                }
            });
        });

        this.serverCommandMapReadySubscription = this.webSocketService.serverCommandMapReady.subscribe(data => {
            const cmpId = this.mainCmpId;
            if (data.id === cmpId) {
                this.serverSideCommandMap = data.map;
            }
        });
        this.commandSubscription = this.eventData.command.subscribe((cmpId: String) => {
            const patch = this.getPatchedData();
            this.webSocketService.doCommand(this.mainCmpId, cmpId, patch);
            if (patch.length > 0) {
                //client data has been sent to server, so reset oldModel
                this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
            }
        });

        this.messageSubscription = this.webSocketService.message.subscribe((args: MessageDlgArgs) => {
            this.boHelperService.messageDialog(this.mainCmpId, args);
        });
        this.changeSubscription = this.eventData.change.subscribe((cmpId: String) => {
            if (this.isServerSideCommand(cmpId)) {
                const patch = this.getPatchedData();
                if (patch.length > 0) {
                    this.webSocketService.doValueChanged(this.mainCmpId, cmpId, patch);
                    //client data has been sent to server, so reset oldModel
                    this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
                }
            }
        });

        this.openDropdownSubscription = this.eventData.openDropdown.subscribe((obj: any) => {
            this.webSocketService.doFillListBox(this.mainCmpId, obj);
        });


    }
    getPatchedData(): any {
        const patch = diff(this.eventData.oldModel, this.eventData.model);
        return patch;
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
        this.messageSubscription.unsubscribe();
    }

    close() {
        super.close();
        this.webSocketService.doCommand(this.mainCmpId, 'ID_FILE_CLOSE');
    }
      isServerSideCommand(idCommand: String) {
        //per ora sono considerati tutti server-side,ma in futuro ci sara la mappa dei comandi che vanno eseguito server side
        return true;
    }

}

