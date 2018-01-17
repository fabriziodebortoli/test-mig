import { BOServiceParams } from './bo.service.params';
import { HttpService } from './http.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from '../../rxjs.imports';

import { apply, diff } from 'json8-patch';

import { MessageDlgArgs, DiagnosticData, MessageDlgResult, DiagnosticDlgResult } from './../../shared/models/message-dialog.model';
import { CommandEventArgs } from './../../shared/models/eventargs.model';

import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { addModelBehaviour } from './../../shared/models/control.model';

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap = [];
    modelStructure = {};
    subscriptions = [];
    boClients = new Array<BOClient>();
    public windowStrings: EventEmitter<any> = new EventEmitter();
    public webSocketService: WebSocketService;
    constructor(params: BOServiceParams, eventData: EventDataService) {
        super(params, eventData);
        this.webSocketService = params.webSocketService;

        this.subscriptions.push(this.webSocketService.modelData.subscribe(data => {
            const models: Array<any> = data.models;
            const cmpId = this.mainCmpId;
            models.forEach(model => {
                if (model.id === cmpId) {
                    if (model.patch) {
                        const patched = apply({ 'data': this.eventData.model }, model.patch);
                        model.data = patched.doc.data;
                    }
                    if (model.data) {
                        for (const prop in model.data) {
                            if (model.data.hasOwnProperty(prop)) {
                                const p = model.data[prop];
                                addModelBehaviour(p);
                                this.eventData.model[prop] = p;
                            }
                        }
                    }
                    this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
                    this.eventData.change.emit('');
                }
            });
        }));

        this.subscriptions.push(this.webSocketService.activationData.subscribe(data => {
            const components: Array<any> = data.components;
            const cmpId = this.mainCmpId;
            components.forEach(cmp => {
                if (cmp.id === cmpId) {
                    this.eventData.activation = cmp.activation;
                }
            });
        }));

        this.subscriptions.push(this.webSocketService.serverCommands.subscribe(data => {
            const cmpId = this.mainCmpId;
            if (data.id === cmpId) {
                this.serverSideCommandMap = data.map;
            }
        }));
        this.subscriptions.push(this.eventData.command.subscribe((args: CommandEventArgs) => {
            const ret = this.onCommand(args.commandId);
            if (ret === true) {
                this.doCommand(args.componentId, args.commandId);
                return;
            }
            if (ret === false) {
                return;
            }
            //se sono observable
            if (ret.subscribe) {
                const subs = ret.subscribe(goOn => {
                    if (goOn) {
                        this.doCommand(args.componentId, args.commandId);
                    }
                    if (subs) {
                        subs.unsubscribe();
                    }

                });
            }
        }));

        this.subscriptions.push(this.webSocketService.message.subscribe((args: MessageDlgArgs) => {
            if (args.cmpId === this.mainCmpId) {
                this.eventData.openMessageDialog.emit(args);
            }
        }));
        this.subscriptions.push(this.webSocketService.diagnostic.subscribe((args: DiagnosticData) => {
            if (args.cmpId === this.mainCmpId) {
                this.eventData.openDiagnosticDialog.emit(args);
            }
        }));

        this.subscriptions.push(this.eventData.radarRecordSelected.subscribe((tbGuid: string) => {
            this.webSocketService.browseRecord(this.mainCmpId, tbGuid);
            this.eventData.change.emit('');
        }));

        this.subscriptions.push(this.eventData.change.subscribe((cmpId: string) => {
            if (!cmpId) {
                return;
            }
            const ret = this.onChange(cmpId);
            if (ret === true) {
                this.doChange(cmpId);
                return;
            }
            if (ret === false) {
                return;
            }
            // se sono observable
            if (ret.subscribe) {
                const subs = ret.subscribe(goOn => {
                    if (goOn) {
                        this.doChange(cmpId);
                    }
                    if (subs) {
                        subs.unsubscribe();
                    }
                });
            }
        }));

        this.subscriptions.push(this.eventData.openDropdown.subscribe((obj: any) => {
            this.webSocketService.doFillListBox(this.mainCmpId, obj);
        }));

        this.subscriptions.push(this.eventData.closeMessageDialog.subscribe((args: MessageDlgResult) => {
            this.webSocketService.doCloseMessageDialog(this.mainCmpId, args);
        }));

        this.subscriptions.push(this.eventData.closeDiagnosticDialog.subscribe((args: DiagnosticDlgResult) => {
            this.webSocketService.doCloseDiagnosticDialog(this.mainCmpId, args);
        }));
        this.subscriptions.push(this.webSocketService.buttonsState.subscribe(data => {
            const result: any = data.response;
            const cmpId = this.mainCmpId;
            if (result.id === cmpId) {
                this.eventData.buttonsState = result.buttonsState;
                this.eventData.change.emit('');
            }
        }));

        this.subscriptions.push(this.webSocketService.behaviours.subscribe(data => {
            const cmpId = this.mainCmpId;
            if (data.response.id === cmpId) {
                this.eventData.behaviours.emit(data.response.behaviours);
                this.eventData.change.emit('');
            }
        }));

        this.subscriptions.push(this.webSocketService.windowStrings.subscribe((args: any) => {
            this.windowStrings.emit(args);
        }));
    }
    getPatchedData(): any {
        const patch = diff(this.eventData.oldModel, this.eventData.model);
        return patch;
    }
    init(cmpId: string) {

        this.boClients.forEach(boClient => {
            boClient.init();
        });
        this.registerModelField('', 'Title');
        this.registerModelField('', 'FormMode');
        this.registerModelField('', 'HeaderStripTitle');
        super.init(cmpId);
        this.webSocketService.checkMessageDialog(this.mainCmpId);
    }
    dispose() {
        super.dispose();
        delete this.serverSideCommandMap;
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        super.close();
        this.webSocketService.closeServerComponent(this.mainCmpId);
    }
    isServerSideCommand(idCommand: string) {
        return this.serverSideCommandMap.indexOf(idCommand) > 0;
    }
    public appendToModelStructure(modelStructure: any) {
        //aggiorna i campi usati dal modello client
        for (let owner in modelStructure) {
            if (!owner) {
                owner = 'global';
            }
            let container = this.modelStructure[owner];
            if (!container) {
                container = modelStructure[owner];
                this.modelStructure[owner] = container;
            }
            else {
                container.push(...modelStructure[owner]);
            }
        }
        //quindi richiede il modello al server, inviandogli nel contempo
        //i campi utilizzati; il server ne terrà traccia ed invierà solo quelli
        this.webSocketService.getDocumentData(this.mainCmpId, this.modelStructure);

    }
    getWindowStrings(cmpId: string, culture: string) {
        this.webSocketService.getWindowStrings(cmpId, culture);
    }
    registerModelField(owner: string, name: string) {
        if (!owner) {
            owner = 'global';
        }
        let container = this.modelStructure[owner];
        if (!container) {
            container = [];
            this.modelStructure[owner] = container;
        }
        container.push(name);
    }
    doCommand(componentId: string, id: string) {
        const patch = this.getPatchedData();
        this.webSocketService.doCommand(
            componentId ? componentId : this.mainCmpId,
            id,
            patch);
        if (patch.length > 0) {
            // client data has been sent to server, so reset oldModel
            this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
        }
    }
    doChange(id: string) {
        if (this.isServerSideCommand(id)) {
            const patch = this.getPatchedData();
            if (patch.length > 0) {
                this.webSocketService.doValueChanged(this.mainCmpId, id, patch);
                // client data has been sent to server, so reset oldModel
                this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
            }
        }
    }


    onCommand(id: string): boolean | Observable<boolean> {
        if (this.boClients.length === 0) {
            return true;
        }

        return Observable.create(observer => {
            this.doEvent(0, 'onCommand', id, observer);
        });
    }

    onChange(id: string): boolean | Observable<boolean> {
        if (this.boClients.length === 0) {
            return true;
        }
        return Observable.create(observer => {
            this.doEvent(0, 'onChange', id, observer);
        });
    }
    /*calls a sequence of events by name, listed in the boservices array, and fails in one of them fails*/
    doEvent(index: number, eventName: string, id: string, observer) {
        const boClient = this.boClients[index];
        const f = boClient[eventName];
        const obs = f.apply(boClient, [id]);
        const subs = obs.subscribe(retVal => {
            if (subs) {
                subs.unsubscribe();
            }
            if (retVal) {//success
                if (++index === this.boClients.length) {
                    observer.next(true);//last event in the list: success for the entire sequence
                    observer.complete();
                } else {
                    //not yet the last event: call next one
                    this.doEvent(index, eventName, id, observer);
                }

            } else {
                //if one event fails, the entire sequence fails
                observer.next(false);
                observer.complete();
            }

        });
    }
}

export class BOClient {
    constructor(
        public boService: BOService) {
    }
    init() {

    }
    onCommand(id: string): Observable<boolean> {
        return Observable.create(observer => {
            observer.next(true);
            observer.complete();
        });
    }
    onChange(id: string): Observable<boolean> {
        return Observable.create(observer => {
            observer.next(true);
            observer.complete();
        });
    }
}

