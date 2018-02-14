import { Injectable, ChangeDetectorRef } from '@angular/core';
import { LayoutService } from './layout.service';
import { TbComponentService } from './tbcomponent.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';
import { DataService } from './data.service';
import { StorageService } from './storage.service';

@Injectable()
export class ComponentMediator implements StorageService {
    public log: Logger;
    public document: DocumentService;

    constructor(
        public layout: LayoutService,
        public tbComponent: TbComponentService,
        public changeDetectorRef: ChangeDetectorRef,
        public eventData: EventDataService,
        public data: DataService,
        public storage: StorageService,
    ) {
        this.log = tbComponent.logger;
        this.document = tbComponent as DocumentService;
    }

    get(key: string): string {
        return this.storage.get(this.uniqueKey(key));
    }

    remove(key: string): void {
        this.storage.remove(this.uniqueKey(key));
    }

    set(key: string, data: string): void {
        this.storage.set(this.uniqueKey(key), data);
    }

    private uniqueKey = key =>
        'settings_components' +
        '_' + (localStorage.getItem('_user') || 'user') +
        '_' + (localStorage.getItem('_company') || 'company') +
        '_' + this.document.mainCmpId +
        '_' + key;
}
