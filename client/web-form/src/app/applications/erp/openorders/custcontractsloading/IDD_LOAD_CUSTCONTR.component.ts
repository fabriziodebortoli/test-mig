import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_CUSTCONTRService } from './IDD_LOAD_CUSTCONTR.service';

@Component({
    selector: 'tb-IDD_LOAD_CUSTCONTR',
    templateUrl: './IDD_LOAD_CUSTCONTR.component.html',
    providers: [IDD_LOAD_CUSTCONTRService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_CUSTCONTRComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_CUSTCONTRService,
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
		boService.appendToModelStructure({'CustContrLoading':['ContractNo','StartValidityDate','EndValidityDate','Customer','FixingDate','Fixing'],'HKLCustSupp':['CustSuppType','CompanyName','Currency'],'HKLCurrency':['Description'],'global':['CustContrLinesLoading','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CustContrLinesLoading':['Select','LineType','Item','Description','UoM','Quantity','UnitValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_CUSTCONTRFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_CUSTCONTRComponent, resolver);
    }
} 