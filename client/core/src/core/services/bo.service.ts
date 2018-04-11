import { BOServiceParams } from './bo.service.params';
import { HttpService } from './http.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from '../../rxjs.imports';

import { MessageDlgArgs, DiagnosticData, MessageDlgResult, DiagnosticDlgResult } from './../../shared/models/message-dialog.model';
import { CommandEventArgs } from './../../shared/models/eventargs.model';

import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { addModelBehaviour, isDataObj } from './../../shared/models/control.model';

@Injectable()
export class BOService extends DocumentService {
    serverSideCommandMap = [];
    subscriptions = [];
    boClients = new Array<BOClient>();
    changedData = {};
    aliases = {};
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
                    if (model.aliases) {
                        this.aliases = model.aliases;
                    }
                    if (model.data) {
                        this.applyPatch(this.eventData.model, model.data, '', true);
                    }
                    this.eventData.change.emit('');
                }
            });
        }));

        this.subscriptions.push(this.webSocketService.activationData.subscribe(data => {
            const components: Array<any> = data.components;
            const cmpId = this.mainCmpId;
            components.forEach(cmp => {
                if (cmp.id === cmpId) {
                    this.applyPatch(this.eventData.activation, cmp.activation, '', true);
                    this.eventData.change.emit();
                    this.eventData.activationChanged.emit();
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
                this.doCommand(args.componentId, args.commandId, '');
                return;
            }
            if (ret === false) {
                return;
            }
            //se sono observable
            if (ret.subscribe) {
                const subs = ret.subscribe(goOn => {
                    if (goOn) {
                        this.doCommand(args.componentId, args.commandId, '');
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

        this.subscriptions.push(this.eventData.checkListBoxAction.subscribe((obj: any) => {
            this.webSocketService.doCheckListBoxAction(this.mainCmpId, obj);
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

        this.subscriptions.push(this.eventData.controlCommand.subscribe((controlId: string) => {
            const patch = this.getPatchedData();
            this.webSocketService.doControlCommand(this.mainCmpId, controlId, patch);
        }));
    }
    public alias(tableOrField: string, field?: string): string {
        let tokens = tableOrField.split('.');
        if (tokens.length == 2) {
            tableOrField = tokens[0];
            field = tokens[1];
        }
        let tableOrFieldObj = this.aliases[tableOrField];
        if (!tableOrFieldObj) {
            return tableOrField;
        }
        if (!field) {
            return tableOrFieldObj.actual;
        }
        let fieldObj = tableOrFieldObj[field];
        if (!fieldObj) {
            fieldObj = { 'actual': field };
        }
        return (tokens.length == 2) ? tableOrFieldObj.actual + '.' + fieldObj.actual : fieldObj.actual;

    }
    getPatchedData(): any {
        const patch = this.changedData;
        this.changedData = {};
        return patch;
    }
    addPrefix(prefix: string, name: string) {
        return prefix ? prefix + '/' + name : name;
    }
    applyPatch(model: any, patch: any, name: string, addEvents: boolean) {

        if (model instanceof Array) {
            if (!(patch instanceof Array)) {
                console.error("Patch data is not an array as expected");
                return;
            }
            let commonElNumber = 0;
            if (model.length > patch.length) {//il server ha meno elementi
                commonElNumber = patch.length; //devo applicare il delta ai primi n elementi
            } else if (model.length < patch.length) { //il server ha più elementi
                commonElNumber = model.length; //devo applicare il delta ai primi n elementi
                //gli altri in più li aggiungo secchi
                for (let i = model.length; i < patch.length; i++) {
                    let item = patch[i];
                    if (addEvents) {
                        addModelBehaviour(item, this.addPrefix(name, '[' + i.toString() + ']'));
                        this.attachEventsToModel(item);
                    }
                    model.push(item);
                }
            } else { //client e server hanno lo stesso numero di elementi
                commonElNumber = model.length;
            }
            //applico il delta agli elementi comuni
            for (let i = 0; i < commonElNumber; i++) {
                this.applyPatch(model[i], patch[i], this.addPrefix(name, '[' + i.toString() + ']'), addEvents);
            }
            if (model.length > patch.length) {//il server ha meno elementi, tolgo quelli in più nel client
                model.splice(commonElNumber, model.length - patch.length);
            }
        }
        else if (isDataObj(model)) {
            for (const prop in patch) {
                model[prop] = patch[prop];
            }
        }
        else {
            for (const prop in patch) {
                let patchVal = patch[prop];
                if (!model.hasOwnProperty(prop)) {
                    model[prop] = patchVal;
                    if (addEvents) {
                        addModelBehaviour(patchVal, this.addPrefix(name, prop));
                        this.attachEventsToModel(patchVal);
                    }
                } else {
                    if (patchVal instanceof Object) {
                        this.applyPatch(model[prop], patchVal, this.addPrefix(name, prop), addEvents);
                    }
                    else {
                        model[prop] = patchVal;
                    }

                }
            }
        }
        if (model.modelChanged) {
            model.modelChanged.emit();
        }
    }
    private indexOfArray(prop: string): number {
        if (prop[0] == '[') {
            return parseInt(prop.substr(1, prop.length - 2));
        }
        return -1;
    }
    private attachEventsToModel(model: any) {
        if (model instanceof Object) {
            if (isDataObj(model)) {//solo se è un dataobj
                this.subscriptions.push(model.valueChanged.subscribe(sender => {
                    const props = sender.name.split('/');
                    let lastIdx = props.length - 1;
                    let obj = this.changedData;
                    for (let j = 0; j < props.length; j++) {
                        let subProp = props[j];
                        if (j == lastIdx) {
                            obj[subProp] = { _value: sender.value };
                        } else {
                            let idx = this.indexOfArray(subProp);
                            if (idx != -1) { //indice di array
                                if (!obj[idx]) {
                                    obj[idx] = {};//per ora non supporto array dentro ad array
                                }
                                obj = obj[idx];
                            } else {
                                if (obj.hasOwnProperty(subProp)) {
                                    obj = obj[subProp];
                                } else {
                                    //quardo se l'elemento successivo è un indice, in tal caso creo un array
                                    let innerSubProp = props[j + 1];
                                    if (this.indexOfArray(innerSubProp) != -1) {
                                        obj[subProp] = [];
                                    }
                                    else {
                                        obj[subProp] = {};

                                    }
                                    obj = obj[subProp];
                                }
                            }
                        }
                    }
                }));
            }
            else {
                for (const prop in model) {
                    this.attachEventsToModel(model[prop]);
                }
            }
        }
    }

    init(cmpId: string) {

        this.boClients.forEach(boClient => {
            boClient.init();
        });
        super.init(cmpId);
        this.webSocketService.checkMessageDialog(cmpId);
        this.webSocketService.getDocumentData(cmpId);
    }
    public dispose() {
        super.dispose();
        delete this.serverSideCommandMap;
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }

    public close() {
        super.close();
        this.webSocketService.closeServerComponent(this.mainCmpId);
    }
    public isServerSideCommand(idCommand: string) {
        return this.serverSideCommandMap.indexOf(idCommand) >= 0;
    }

    public getWindowStrings(cmpId: string, culture: string) {
        this.webSocketService.getWindowStrings(cmpId, culture);
    }

    public getActivationData(cmpId: string) {
        this.webSocketService.getActivationData(cmpId);
    }

    public activateContainer(id: string, active: boolean, isTileGroup: boolean) {
        this.webSocketService.activateContainer(this.mainCmpId, id, active, isTileGroup);
    }
    public doCommand(componentId: string, commandId: string, controlId: string) {
        const patch = this.getPatchedData();
        this.webSocketService.doCommand(
            componentId ? componentId : this.mainCmpId,
            commandId,
            controlId,
            patch);
        console.log("doCommand", patch);

    }
    public doChange(id: string) {
        if (this.isServerSideCommand(id)) {
            const patch = this.getPatchedData();
            this.webSocketService.doValueChanged(this.mainCmpId, id, patch);
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

