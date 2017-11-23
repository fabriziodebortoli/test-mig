import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRANSFERTPLService } from './IDD_TRANSFERTPL.service';

@Component({
    selector: 'tb-IDD_TRANSFERTPL',
    templateUrl: './IDD_TRANSFERTPL.component.html',
    providers: [IDD_TRANSFERTPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TRANSFERTPLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TRANSFERTPLService,
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
		boService.appendToModelStructure({'TransferTpl':['Template','Description','Priority','ValidityStartingDate','ValidityEndingDate'],'global':['TransferTplOrigin','TransferTplDest','TotPercCostCenters','TotPercJobs','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'TransferTplOrigin':['CostCenter','Detailed'],'HKLOriginCostCenter':['Description'],'TransferTplDest':['CostCenter','Job','ProductLine','TransferPerc','NotToBePosted'],'HKLDestCostCenter':['Description'],'HKLDestJob':['Description'],'HKLDestProductLine':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRANSFERTPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRANSFERTPLComponent, resolver);
    }
} 