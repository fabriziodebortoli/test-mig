import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOLETOSService } from './IDD_BOLETOS.service';

@Component({
    selector: 'tb-IDD_BOLETOS',
    templateUrl: './IDD_BOLETOS.component.html',
    providers: [IDD_BOLETOSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOLETOSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOLETOSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'Boletos':['BoletoNo','Cancelled','IssuerBank','BarCode','OurNumber','PrintedOurNumber','Amount','DueDate','IssuingDate','ConditionCode','InterestRate','PenalityRate','DiscountRate','ProtestDays','Instruction','PrintedOnPaper','PrintedOnFile','UpdateNo','OriginalAmount','Customer','Collected','CollectionDate','CollectedAmount','InterestPenality'],'HKLIssuerBank':['Description'],'HKLCustomers':['CompanyName'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOLETOSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOLETOSComponent, resolver);
    }
} 