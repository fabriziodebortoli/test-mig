import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMS_WAP_TRANSACTIONSService } from './IDD_ITEMS_WAP_TRANSACTIONS.service';

@Component({
    selector: 'tb-IDD_ITEMS_WAP_TRANSACTIONS',
    templateUrl: './IDD_ITEMS_WAP_TRANSACTIONS.component.html',
    providers: [IDD_ITEMS_WAP_TRANSACTIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMS_WAP_TRANSACTIONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMS_WAP_TRANSACTIONSService,
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
		boService.appendToModelStructure({'Items':['Item','Description','BaseUoM','IsGood'],'global':['ItemsWAPTransactions','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ItemsWAPTransactions':['StartingPeriodDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMS_WAP_TRANSACTIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMS_WAP_TRANSACTIONSComponent, resolver);
    }
} 