import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ENTRYSALESPERSON_FULLService } from './IDD_ENTRYSALESPERSON_FULL.service';

@Component({
    selector: 'tb-IDD_ENTRYSALESPERSON_FULL',
    templateUrl: './IDD_ENTRYSALESPERSON_FULL.component.html',
    providers: [IDD_ENTRYSALESPERSON_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ENTRYSALESPERSON_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ENTRYSALESPERSON_FULLService,
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
        
        		this.bo.appendToModelStructure({'CommissionsEntries':['Salesperson','AccrualPercAtInvoiceDate','Policy','Area','AccrualType','CommTotAmount','DocNo','DocumentDate','TaxableAmount','TotalAmount','Customer','Notes'],'HKLSalesperson':['Name'],'HKLArea':['Description'],'HKLCustSupp':['CompNameComplete'],'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ENTRYSALESPERSON_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ENTRYSALESPERSON_FULLComponent, resolver);
    }
} 