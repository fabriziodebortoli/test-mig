import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CHANGERETAILDATAService } from './IDD_CHANGERETAILDATA.service';

@Component({
    selector: 'tb-IDD_CHANGERETAILDATA',
    templateUrl: './IDD_CHANGERETAILDATA.component.html',
    providers: [IDD_CHANGERETAILDATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CHANGERETAILDATAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CHANGERETAILDATAService,
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
		boService.appendToModelStructure({'DBTChangeRetailData':['ChangeRetailDataNo','ChangeRetailDataDate','PostingDate','AccReason','AccTemplate','ForValue','ForTaxChange','TotNetPriceDiff','TotVATDiff','GrandTotDiff','Storage'],'HKLAccRsn':['Description'],'HKLAccTpl':['Description'],'HKLStorage':['Description'],'global':['DBTChangeRetailDataDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTChangeRetailDataDetail':['Item','CurrentPrice','NewPrice','Qty','BaseUoM','VATDiff','TotDiff'],'HKLItem':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CHANGERETAILDATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CHANGERETAILDATAComponent, resolver);
    }
} 