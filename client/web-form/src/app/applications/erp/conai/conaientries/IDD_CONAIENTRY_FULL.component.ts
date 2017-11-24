import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONAIENTRY_FULLService } from './IDD_CONAIENTRY_FULL.service';

@Component({
    selector: 'tb-IDD_CONAIENTRY_FULL',
    templateUrl: './IDD_CONAIENTRY_FULL.component.html',
    providers: [IDD_CONAIENTRY_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CONAIENTRY_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CONAIENTRY_FULLService,
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
        
        		this.bo.appendToModelStructure({'ConaiEntries':['EntryId','EntryDate','Customer','ExemptionPerc','Item','PrimaryPackage','Material','PackageType','PackageTypeDescription','UnitContribution','TotalContributionAmount','Qty','ExemptQty','SubjectedQty','PrimaryPackageQty','SecondaryTertiaryPackageQty','DocumentType','DocumentLine','DocumentNumber','DocumentDate'],'HKLCustomer':['CompNameComplete'],'HKLMaterials':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONAIENTRY_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONAIENTRY_FULLComponent, resolver);
    }
} 