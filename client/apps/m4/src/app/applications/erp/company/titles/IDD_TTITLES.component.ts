﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_TTITLESService } from './IDD_TTITLES.service';

@Component({
    selector: 'tb-IDD_TTITLES',
    templateUrl: './IDD_TTITLES.component.html',
    providers: [IDD_TTITLESService, ComponentInfoService]
})
export class IDD_TTITLESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TTITLESService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'Titles':['TitleCode','Description'],'global':['__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_TTITLESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TTITLESComponent, resolver);
    }
} 