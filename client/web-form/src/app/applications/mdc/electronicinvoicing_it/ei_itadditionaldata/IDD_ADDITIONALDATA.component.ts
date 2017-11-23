import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ADDITIONALDATAService } from './IDD_ADDITIONALDATA.service';

@Component({
    selector: 'tb-IDD_ADDITIONALDATA',
    templateUrl: './IDD_ADDITIONALDATA.component.html',
    providers: [IDD_ADDITIONALDATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ADDITIONALDATAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ADDITIONALDATAService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllCustomers','CurrVersion','FileVersion','bLoadAllNodes','bTreeView','CheckOnForAllData','EI_ITCustSuppAdditionalData','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'EI_ITCustSuppAddData':['CustSupp'],'EI_ITCustSuppAdditionalData':['LocalFieldName','FieldMessage']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ADDITIONALDATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ADDITIONALDATAComponent, resolver);
    }
} 