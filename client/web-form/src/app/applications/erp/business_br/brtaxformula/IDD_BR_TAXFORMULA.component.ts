import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXFORMULAService } from './IDD_BR_TAXFORMULA.service';

@Component({
    selector: 'tb-IDD_BR_TAXFORMULA',
    templateUrl: './IDD_BR_TAXFORMULA.component.html',
    providers: [IDD_BR_TAXFORMULAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAXFORMULAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_TAXFORMULAService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRTaxFormula':['TaxFormulaCode','Description','TaxType','ValidityStartingDate','ValidityEndingDate','TaxableAmountFormula','TaxAmountFormula'],'global':['bTaxableAmountEnable','bTaxEnable','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXFORMULAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXFORMULAComponent, resolver);
    }
} 