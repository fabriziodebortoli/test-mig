import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RSNDETAILService } from './IDD_RSNDETAIL.service';

@Component({
    selector: 'tb-IDD_RSNDETAIL',
    templateUrl: './IDD_RSNDETAIL.component.html',
    providers: [IDD_RSNDETAILService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RSNDETAILComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RSNDETAILService,
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
		boService.appendToModelStructure({'AccountingReasons':['Reason','Description','UseForPureEntry','UseForSaleEntry','UseForPurchaseEntry','UseForRetailSaleEntry','AGOAccReason','OMNIAAccReason'],'global':['Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Notes':['Notes'],'HKLAGOAccReasons':['Description'],'HKLOMNIAAccReasons':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RSNDETAILFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RSNDETAILComponent, resolver);
    }
} 