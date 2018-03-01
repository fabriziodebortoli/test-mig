import { Injectable, ChangeDetectorRef, ElementRef, Optional } from '@angular/core';
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
        @Optional() private cmpInfoService: ComponentInfoService,
        @Optional() private elRef: ElementRef
    ) {
        this.log = tbComponent.logger;
        this.document = tbComponent as DocumentService;
        this.storage.options.componentInfo = {
            ...this.storage.options.componentInfo,
            type: elRef && elRef.nativeElement &&
                (elRef.nativeElement.localName || elRef.nativeElement.nodeName || elRef.nativeElement.tagName),
            app: get(cmpInfoService , 'componentInfo.app'),
            mod: get(cmpInfoService, 'componentInfo.mod')
        };
    }
}
