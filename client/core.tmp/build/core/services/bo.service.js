import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { WebSocketService } from './websocket.service';
import { BOHelperService } from './bohelper.service';
import { apply, diff } from 'json8-patch';
export class BOService extends DocumentService {
    /**
     * @param {?} webSocketService
     * @param {?} boHelperService
     * @param {?} eventData
     */
    constructor(webSocketService, boHelperService, eventData) {
        super(boHelperService.logger, eventData);
        this.webSocketService = webSocketService;
        this.boHelperService = boHelperService;
        this.serverSideCommandMap = [];
        this.modelStructure = {};
        this.subscriptions = [];
        this.boClients = new Array();
        this.subscriptions.push(this.webSocketService.modelData.subscribe(data => {
            const models = data.models;
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
            const components = data.components;
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
        this.subscriptions.push(this.eventData.command.subscribe((cmpId) => {
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
        this.subscriptions.push(this.webSocketService.message.subscribe((args) => {
            if (args.cmpId === this.mainCmpId) {
                this.eventData.openMessageDialog.emit(args);
            }
        }));
        this.subscriptions.push(this.eventData.change.subscribe((cmpId) => {
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
        this.subscriptions.push(this.eventData.openDropdown.subscribe((obj) => {
            this.webSocketService.doFillListBox(this.mainCmpId, obj);
        }));
        this.subscriptions.push(this.eventData.closeMessageDialog.subscribe((args) => {
            this.webSocketService.doCloseMessageDialog(this.mainCmpId, args);
        }));
        this.subscriptions.push(this.webSocketService.buttonsState.subscribe(data => {
            const result = data.response;
            const cmpId = this.mainCmpId;
            if (result.id === cmpId) {
                this.eventData.buttonsState = result.buttonsState;
            }
        }));
    }
    /**
     * @return {?}
     */
    getPatchedData() {
        const /** @type {?} */ patch = diff(this.eventData.oldModel, this.eventData.model);
        return patch;
    }
    /**
     * @param {?} cmpId
     * @return {?}
     */
    init(cmpId) {
        this.boClients.forEach(boClient => {
            boClient.init();
        });
        this.registerModelField('', 'Title');
        super.init(cmpId);
        this.webSocketService.getDocumentData(this.mainCmpId, this.modelStructure);
        this.webSocketService.checkMessageDialog(this.mainCmpId);
    }
    /**
     * @return {?}
     */
    dispose() {
        super.dispose();
        delete this.serverSideCommandMap;
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }
    /**
     * @return {?}
     */
    close() {
        super.close();
        this.webSocketService.doCommand(this.mainCmpId, 'ID_FILE_CLOSE');
    }
    /**
     * @param {?} idCommand
     * @return {?}
     */
    isServerSideCommand(idCommand) {
        return this.serverSideCommandMap.includes(idCommand);
    }
    /**
     * @param {?} owner
     * @param {?} name
     * @return {?}
     */
    registerModelField(owner, name) {
        if (!owner) {
            owner = 'global';
        }
        let /** @type {?} */ container = this.modelStructure[owner];
        if (!container) {
            container = [];
            this.modelStructure[owner] = container;
        }
        container.push(name);
    }
    /**
     * @param {?} id
     * @return {?}
     */
    doCommand(id) {
        const /** @type {?} */ patch = this.getPatchedData();
        this.webSocketService.doCommand(this.mainCmpId, id, patch);
        if (patch.length > 0) {
            // client data has been sent to server, so reset oldModel
            this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
        }
    }
    /**
     * @param {?} id
     * @return {?}
     */
    doChange(id) {
        if (this.isServerSideCommand(id)) {
            const /** @type {?} */ patch = this.getPatchedData();
            if (patch.length > 0) {
                this.webSocketService.doValueChanged(this.mainCmpId, id, patch);
                // client data has been sent to server, so reset oldModel
                this.eventData.oldModel = JSON.parse(JSON.stringify(this.eventData.model));
            }
        }
    }
    /**
     * @param {?} id
     * @return {?}
     */
    onCommand(id) {
        if (this.boClients.length === 0) {
            return true;
        }
        return Observable.create(observer => {
            this.doEvent(0, 'onCommand', id, observer);
        });
    }
    /**
     * @param {?} id
     * @return {?}
     */
    onChange(id) {
        if (this.boClients.length === 0) {
            return true;
        }
        return Observable.create(observer => {
            this.doEvent(0, 'onChange', id, observer);
        });
    }
    /**
     * @param {?} index
     * @param {?} eventName
     * @param {?} id
     * @param {?} observer
     * @return {?}
     */
    doEvent(index, eventName, id, observer) {
        const /** @type {?} */ boClient = this.boClients[index];
        const /** @type {?} */ f = boClient[eventName];
        const /** @type {?} */ obs = f.apply(boClient, [id]);
        const /** @type {?} */ subs = obs.subscribe(retVal => {
            if (subs) {
                subs.unsubscribe();
            }
            if (retVal) {
                if (++index === this.boClients.length) {
                    observer.next(true); //last event in the list: success for the entire sequence
                    observer.complete();
                }
                else {
                    //not yet the last event: call next one
                    this.doEvent(index, eventName, id, observer);
                }
            }
            else {
                //if one event fails, the entire sequence fails
                observer.next(false);
                observer.complete();
            }
        });
    }
}
BOService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
BOService.ctorParameters = () => [
    { type: WebSocketService, },
    { type: BOHelperService, },
    { type: EventDataService, },
];
function BOService_tsickle_Closure_declarations() {
    /** @type {?} */
    BOService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    BOService.ctorParameters;
    /** @type {?} */
    BOService.prototype.serverSideCommandMap;
    /** @type {?} */
    BOService.prototype.modelStructure;
    /** @type {?} */
    BOService.prototype.subscriptions;
    /** @type {?} */
    BOService.prototype.boClients;
    /** @type {?} */
    BOService.prototype.webSocketService;
    /** @type {?} */
    BOService.prototype.boHelperService;
}
export class BOClient {
    /**
     * @param {?} boService
     */
    constructor(boService) {
        this.boService = boService;
    }
    /**
     * @return {?}
     */
    init() {
    }
    /**
     * @param {?} id
     * @return {?}
     */
    onCommand(id) {
        return Observable.create(observer => {
            observer.next(true);
            observer.complete();
        });
    }
    /**
     * @param {?} id
     * @return {?}
     */
    onChange(id) {
        return Observable.create(observer => {
            observer.next(true);
            observer.complete();
        });
    }
}
function BOClient_tsickle_Closure_declarations() {
    /** @type {?} */
    BOClient.prototype.boService;
}
