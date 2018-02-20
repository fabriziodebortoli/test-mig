import { Injectable, ChangeDetectorRef } from '@angular/core';
import { LayoutService } from './layout.service';
import { TbComponentService } from './tbcomponent.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';
import { DataService } from './data.service';
import { StorageService } from './storage.service';
import { ComponentInfoService } from './component-info.service';

export type StorageOptions = { scope?: KeyScope, cmpId?: string };
export enum KeyScope { Component, Document, Global };

@Injectable()
export class ComponentMediator implements StorageService {
    public log: Logger;
    public document: DocumentService;
    private storageOpt: StorageOptions = { scope: KeyScope.Component };

    constructor(
        public layout: LayoutService,
        public tbComponent: TbComponentService,
        public changeDetectorRef: ChangeDetectorRef,
        public eventData: EventDataService,
        public data: DataService,
        public storage: StorageService,
        private cmpInfoService: ComponentInfoService
    ) {
        this.log = tbComponent.logger;
        this.document = tbComponent as DocumentService;
    }

    setStorage(opt: StorageOptions) {
        this.storageOpt = { ...this.storageOpt, ...opt };
    }

    get<T>(key: string): T;
    get(key: string): any {
        return this.storage.get(this.uniqueKey(key, this.storageOpt.scope));
    }

    remove(key: string): void {
        this.storage.remove(this.uniqueKey(key, this.storageOpt.scope));
    }

    set(key: string, data: any): void {
        this.storage.set(this.uniqueKey(key, this.storageOpt.scope), data);
    }

    private uniqueKey = (key, per) => this.uniqueKeyPer[per](key);

    // tslint:disable-next-line:member-ordering
    private uniqueKeyPer = {
        [KeyScope.Component]: key => [
            localStorage.getItem('_user'),
            localStorage.getItem('_company'),
            this.cmpInfoService.componentInfo.app,
            this.cmpInfoService.componentInfo.mod,
            this.storageOpt.cmpId,
            key].join('_'),
        [KeyScope.Document]: key => '',
        [KeyScope.Global]: key => ''
    };
}
