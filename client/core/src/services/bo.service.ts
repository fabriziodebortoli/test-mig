import { Injectable } from '@angular/core';

import { Observable } from 'rxjs/Rx';

import { MessageDlgArgs, MessageDlgResult } from './../shared/containers/message-dialog/message-dialog.component';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { BOHelperService } from './bohelper.service';

import { apply, diff } from 'json8-patch';

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap = [];
    modelStructure = {};
    subscriptions = [];
    boClients = new Array<BOClient>();
    constructor(
        private webSocketService: WebSocketService,
        public boHelperService: BOHelperService,
        eventData: EventDataService) {
        super(boHelperService.logger, eventData);

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
                                this.eventData.model[prop] = model.data[prop];
                            }
                        }
                    }
                    this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
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
        this.subscriptions.push(this.eventData.command.subscribe((cmpId: string) => {
            const ret = this.onCommand(cmpId);
            if (ret === true) {
                this.doCommand(cmpId);
                return;
            }
            if (ret === false) {
                return;
            }
            //se sono observable
            if (ret.subscribe) {
                const subs = ret.subscribe(goOn => {
                    if (goOn) {
                        this.doCommand(cmpId);
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
        this.subscriptions.push(this.eventData.change.subscribe((cmpId: string) => {
            const ret = this.onChange(cmpId);
            if (ret === true) {
                this.doChange(cmpId);
                return;
            }
            if (ret === false) {
                return;
            }
            //se sono observable
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

        this.subscriptions.push(this.webSocketService.buttonsState.subscribe(data => {
            const result: any = data.response;
            const cmpId = this.mainCmpId;
            if (result.id === cmpId) {
                this.eventData.buttonsState = result.buttonsState;
            }
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
        super.init(cmpId);
        this.webSocketService.getDocumentData(this.mainCmpId, this.modelStructure);
        this.webSocketService.checkMessageDialog(this.mainCmpId);
    }
    dispose() {
        super.dispose();
        delete this.serverSideCommandMap;
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    close() {
        super.close();
        this.webSocketService.doCommand(this.mainCmpId, 'ID_FILE_CLOSE');
    }
    isServerSideCommand(idCommand: string) {
        return this.serverSideCommandMap.includes(idCommand);
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
    doCommand(id: string) {
        const patch = this.getPatchedData();
        this.webSocketService.doCommand(this.mainCmpId, id, patch);
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
        protected boService: BOService) {
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

