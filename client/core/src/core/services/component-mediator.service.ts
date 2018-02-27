import { Injectable, ChangeDetectorRef, ElementRef } from '@angular/core';
import { LayoutService } from './layout.service';
import { TbComponentService } from './tbcomponent.service';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';
import { DataService } from './data.service';
import { StorageService } from './storage.service';
import { ComponentInfoService } from './component-info.service';
import { ControlComponent } from './../../shared/controls/control.component';


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
        private cmpInfoService: ComponentInfoService,
        private elRef: ElementRef,
    ) {
        this.log = tbComponent.logger;
        this.document = tbComponent as DocumentService;
        this.storage.options.componentInfo = {
            ...this.storage.options.componentInfo,
            type: elRef.nativeElement.localName || elRef.nativeElement.nodeName || elRef.nativeElement.tagName,
            app: cmpInfoService.componentInfo.app,
            mod: cmpInfoService.componentInfo.mod
        };
    }
}
