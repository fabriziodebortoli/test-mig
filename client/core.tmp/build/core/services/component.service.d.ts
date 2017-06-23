import { Type, ComponentFactoryResolver, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { Logger } from './logger.service';
import { HttpService } from './http.service';
import { UtilsService } from './utils.service';
import { WebSocketService } from './websocket.service';
import { ComponentInfo } from '../../shared/models/component.info';
export declare class ComponentService {
    private router;
    private webSocketService;
    private httpService;
    private logger;
    private utils;
    components: Array<ComponentInfo>;
    componentsToCreate: any[];
    currentComponentId: string;
    creatingComponent: boolean;
    subscriptions: any[];
    componentInfoCreated: EventEmitter<ComponentCreatedArgs>;
    componentInfoAdded: EventEmitter<ComponentInfo>;
    componentInfoRemoved: EventEmitter<ComponentInfo>;
    componentCreationError: EventEmitter<string>;
    activateComponent: boolean;
    constructor(router: Router, webSocketService: WebSocketService, httpService: HttpService, logger: Logger, utils: UtilsService);
    argsToString(args: any): string;
    createReportComponent(ns: string, activate: boolean, args?: any): void;
    dispose(): void;
    createNextComponent(): void;
    addComponent<T>(component: ComponentInfo): void;
    removeComponent(component: ComponentInfo): void;
    removeComponentById(componentId: string): void;
    createComponentFromUrl(url: string, activate: boolean): void;
    createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver, args?: any): void;
    onComponentCreated(info: ComponentInfo): void;
}
export declare class ComponentCreatedArgs {
    index: Number;
    activate: boolean;
    constructor(index: Number, activate: boolean);
}
