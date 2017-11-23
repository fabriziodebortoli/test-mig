import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LANGUAGES_FRAMEService } from './IDD_LANGUAGES_FRAME.service';

@Component({
    selector: 'tb-IDD_LANGUAGES_FRAME',
    templateUrl: './IDD_LANGUAGES_FRAME.component.html',
    providers: [IDD_LANGUAGES_FRAMEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LANGUAGES_FRAMEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_LANGUAGES_CULTURE_itemSource: any;

    constructor(document: IDD_LANGUAGES_FRAMEService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_LANGUAGES_CULTURE_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Languages.Culture"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Languages':['Language','Description','Culture'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LANGUAGES_FRAMEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LANGUAGES_FRAMEComponent, resolver);
    }
} 