import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_IMPORT_TAXRULESService } from './IDD_BR_IMPORT_TAXRULES.service';

@Component({
    selector: 'tb-IDD_BR_IMPORT_TAXRULES',
    templateUrl: './IDD_BR_IMPORT_TAXRULES.component.html',
    providers: [IDD_BR_IMPORT_TAXRULESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_IMPORT_TAXRULESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_IMPORT_TAXRULESService,
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
		boService.appendToModelStructure({'DBTBRImportTaxRules':['TaxRuleCode','Description','Priority','ValidityStartingDate','ValidityEndingDate','AllItems','Item','NCM','ItemFiscalCtg','NotaFiscalCode','OriginalCFOP','CustSuppFiscalCtg','CFOP','ICMSTaxCode','ICMSType','ICMSSTTaxCode','ICMSSTType','COFINSTaxCode','COFINSType','IPITaxCode','IPIType','PISTaxCode','PISType','SIMPLESTaxCode'],'HKLItems':['Description'],'HKLBRNCM':['Description'],'HKLItemFiscalCtg':['Description'],'HKLBRNotaFiscalType':['Description'],'HKLCustSuppFiscalCtg':['Description'],'HKLBRCFOP':['Description'],'HKLBRTaxCodeICMS':['Description'],'HKLBRTaxCodeICMSST':['Description'],'HKLBRTaxCodeCOFINS':['Description'],'HKLBRTaxCodeIPI':['Description'],'HKLBRTaxCodePIS':['Description'],'HKLBRTaxCodeSIMPLES':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_IMPORT_TAXRULESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_IMPORT_TAXRULESComponent, resolver);
    }
} 