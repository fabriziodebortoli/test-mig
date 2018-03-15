import { Injectable, ChangeDetectorRef, ElementRef, Optional, Injector } from '@angular/core';
import { LayoutService } from './layout.service';
import { TbComponentService } from './tbcomponent.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';
import { DataService } from './data.service';
import { StorageService } from './storage.service';
import { ComponentInfoService } from './component-info.service';
import { ControlComponent } from './../../shared/controls/control.component';
import { get } from 'lodash';

@Injectable()
export class ComponentMediator {
    public log: Logger;
    public document: DocumentService;

    constructor(
        public layout: LayoutService,
        public tbComponent: TbComponentService,
        public changeDetectorRef: ChangeDetectorRef,
        public eventData: EventDataService,
        public data: DataService,
        public storage: StorageService,
        @Optional() private injector: Injector,
    ) {
        this.log = tbComponent.logger;
        this.document = tbComponent as DocumentService;
    }
}
