import { Injectable, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { Logger } from './logger.service';
import { HttpService } from './http.service';
import { UtilsService } from './utils.service';
import { WebSocketService } from './websocket.service';
import { ComponentInfo } from '../../shared/models/component.info';
export class ComponentService {
    /**
     * @param {?} router
     * @param {?} webSocketService
     * @param {?} httpService
     * @param {?} logger
     * @param {?} utils
     */
    constructor(router, webSocketService, httpService, logger, utils) {
        this.router = router;
        this.webSocketService = webSocketService;
        this.httpService = httpService;
        this.logger = logger;
        this.utils = utils;
        this.components = [];
        this.componentsToCreate = new Array();
        this.creatingComponent = false; //semaforo
        this.subscriptions = [];
        this.componentInfoCreated = new EventEmitter();
        this.componentInfoAdded = new EventEmitter();
        this.componentInfoRemoved = new EventEmitter();
        this.componentCreationError = new EventEmitter();
        this.activateComponent = false;
        this.subscriptions.push(this.webSocketService.windowOpen.subscribe(data => {
            this.componentsToCreate.push(...data.components);
            this.createNextComponent();
        }));
        this.subscriptions.push(this.webSocketService.windowClose.subscribe(data => {
            if (data && data.id) {
                this.removeComponentById(data.id);
            }
        }));
    }
    /**
     * @param {?} args
     * @return {?}
     */
    argsToString(args) {
        if (typeof (args) === 'object') {
            if (Object.keys(args).length) {
                return JSON.stringify(args);
            }
            else {
                return undefined;
            }
        }
        else {
            return undefined;
        }
    }
    /**
     * @param {?} ns
     * @param {?} activate
     * @param {?=} args
     * @return {?}
     */
    createReportComponent(ns, activate, args = undefined) {
        let /** @type {?} */ url = 'rs/reportingstudio/' + ns + '/';
        args = this.argsToString(args);
        if (args !== undefined) {
            url += args;
        }
        this.createComponentFromUrl(url, activate);
    }
    /**
     * @return {?}
     */
    dispose() {
        this.subscriptions.forEach(subs => subs.unsubscribe());
    }
    /**
     * @return {?}
     */
    createNextComponent() {
        if (this.creatingComponent) {
            return;
        }
        if (this.componentsToCreate.length === 0) {
            this.currentComponentId = undefined;
            return;
        }
        this.creatingComponent = true;
        const /** @type {?} */ cmp = this.componentsToCreate.pop();
        this.currentComponentId = cmp.id;
        let /** @type {?} */ url = cmp.app.toLowerCase() + '/' + cmp.mod.toLowerCase() + '/' + cmp.name;
        const /** @type {?} */ args = this.argsToString(cmp.args);
        if (args !== undefined) {
            url += '/' + args;
        }
        this.createComponentFromUrl(url, true);
    }
    /**
     * @template T
     * @param {?} component
     * @return {?}
     */
    addComponent(component) {
        this.components.push(component);
        this.componentInfoAdded.emit(component);
    }
    /**
     * @param {?} component
     * @return {?}
     */
    removeComponent(component) {
        let /** @type {?} */ idx = this.components.indexOf(component);
        if (idx === -1) {
            console.debug('ComponentService: cannot remove conponent with id ' + component.id + ' because it does not exist');
            return;
        }
        this.components.splice(idx, 1);
        this.componentInfoRemoved.emit(component);
    }
    /**
     * @param {?} componentId
     * @return {?}
     */
    removeComponentById(componentId) {
        let /** @type {?} */ removed;
        let /** @type {?} */ idx = -1;
        for (let /** @type {?} */ i = 0; i < this.components.length; i++) {
            const /** @type {?} */ comp = this.components[i];
            if (comp.id === componentId) {
                idx = i;
                removed = comp;
                break;
            }
        }
        if (idx === -1) {
            console.debug('ComponentService: cannot remove conponent with id ' + componentId + ' because it does not exist');
            return;
        }
        this.components.splice(idx, 1);
        this.componentInfoRemoved.emit(removed);
    }
    /**
     * @param {?} url
     * @param {?} activate
     * @return {?}
     */
    createComponentFromUrl(url, activate) {
        this.activateComponent = activate;
        this.router.navigate([{ outlets: { dynamic: 'proxy/' + url }, skipLocationChange: false, replaceUrl: false }])
            .then(success => {
            this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }]).then(success1 => {
                this.creatingComponent = false;
                this.createNextComponent();
            });
        })
            .catch(reason => {
            console.log(reason);
            this.componentCreationError.emit(reason);
        });
    }
    /**
     * @template T
     * @param {?} component
     * @param {?} resolver
     * @param {?=} args
     * @return {?}
     */
    createComponent(component, resolver, args = {}) {
        const /** @type {?} */ info = new ComponentInfo();
        info.id = this.currentComponentId ? this.currentComponentId : this.utils.generateGUID();
        info.factory = resolver.resolveComponentFactory(component);
        info.args = args;
        this.addComponent(info);
    }
    /**
     * @param {?} info
     * @return {?}
     */
    onComponentCreated(info) {
        this.componentInfoCreated.emit(new ComponentCreatedArgs(this.components.indexOf(info), this.activateComponent));
    }
}
ComponentService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
ComponentService.ctorParameters = () => [
    { type: Router, },
    { type: WebSocketService, },
    { type: HttpService, },
    { type: Logger, },
    { type: UtilsService, },
];
function ComponentService_tsickle_Closure_declarations() {
    /** @type {?} */
    ComponentService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ComponentService.ctorParameters;
    /** @type {?} */
    ComponentService.prototype.components;
    /** @type {?} */
    ComponentService.prototype.componentsToCreate;
    /** @type {?} */
    ComponentService.prototype.currentComponentId;
    /** @type {?} */
    ComponentService.prototype.creatingComponent;
    /** @type {?} */
    ComponentService.prototype.subscriptions;
    /** @type {?} */
    ComponentService.prototype.componentInfoCreated;
    /** @type {?} */
    ComponentService.prototype.componentInfoAdded;
    /** @type {?} */
    ComponentService.prototype.componentInfoRemoved;
    /** @type {?} */
    ComponentService.prototype.componentCreationError;
    /** @type {?} */
    ComponentService.prototype.activateComponent;
    /** @type {?} */
    ComponentService.prototype.router;
    /** @type {?} */
    ComponentService.prototype.webSocketService;
    /** @type {?} */
    ComponentService.prototype.httpService;
    /** @type {?} */
    ComponentService.prototype.logger;
    /** @type {?} */
    ComponentService.prototype.utils;
}
export class ComponentCreatedArgs {
    /**
     * @param {?} index
     * @param {?} activate
     */
    constructor(index, activate) {
        this.index = index;
        this.activate = activate;
    }
}
function ComponentCreatedArgs_tsickle_Closure_declarations() {
    /** @type {?} */
    ComponentCreatedArgs.prototype.index;
    /** @type {?} */
    ComponentCreatedArgs.prototype.activate;
}
