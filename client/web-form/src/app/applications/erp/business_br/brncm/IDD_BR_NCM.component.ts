import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_NCMService } from './IDD_BR_NCM.service';

@Component({
    selector: 'tb-IDD_BR_NCM',
    templateUrl: './IDD_BR_NCM.component.html',
    providers: [IDD_BR_NCMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_NCMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_NCMService,
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
        
        		this.bo.appendToModelStructure({'DBTBRNCM':['NCM','Description','ICMSTaxRateCode','IPISettlementType','ValidityStartingDate','ValidityEndingDate','ApproxTaxesImportPerc','StateApproxTaxesImportPerc','MunApproxTaxesImportPerc','ApproxTaxesDomesticPerc','StateApproxTaxesDomesticPerc','MunApproxTaxesDomesticPerc'],'HKLBRTaxRateCode':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_NCMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_NCMComponent, resolver);
    }
} 