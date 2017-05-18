import { Observable } from 'rxjs/Rx';
import { MessageDlgArgs, MessageDlgResult } from './../shared/containers/message-dialog/message-dialog.component';
import { UtilsService } from './utils.service';
import { Injectable } from '@angular/core';

import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { apply, diff } from 'json8-patch';
import { BOHelperService } from 'app/core/bohelper.service';

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap = [];

    subscriptions = [];

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
                        for (let prop in model.data) {
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
                    subs.unsubscribe();
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
                    subs.unsubscribe();
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
        super.init(cmpId);
        this.webSocketService.getDocumentData(this.mainCmpId);
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
    onCommand(id: string): boolean | Observable<boolean> {
        return true;
    }
    doCommand(id: string) {
        const patch = this.getPatchedData();
        this.webSocketService.doCommand(this.mainCmpId, id, patch);
        if (patch.length > 0) {
            // client data has been sent to server, so reset oldModel
            this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
        }
    }
    onChange(id: string): boolean | Observable<boolean> {
        return true;
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
}

