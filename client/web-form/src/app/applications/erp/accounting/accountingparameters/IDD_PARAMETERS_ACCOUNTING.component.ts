import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERS_ACCOUNTINGService } from './IDD_PARAMETERS_ACCOUNTING.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_ACCOUNTING',
    templateUrl: './IDD_PARAMETERS_ACCOUNTING.component.html',
    providers: [IDD_PARAMETERS_ACCOUNTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PARAMETERS_ACCOUNTINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ACCPARAMDECL_TELEMATICDECLARATIONTYPE_itemSource: any;
public IDC_TBA_CODETYPE_itemSource: any;

    constructor(document: IDD_PARAMETERS_ACCOUNTINGService,
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
        this.IDC_ACCPARAMDECL_TELEMATICDECLARATIONTYPE_itemSource = {
  "name": "DeclarationFileEnumCombo",
  "namespace": "ERP.Accounting.Components.DeclarationExportFileEnumCombo"
}; 
this.IDC_TBA_CODETYPE_itemSource = {
  "name": "FiscalPrintoutsEnumCombo",
  "namespace": "ERP.Accounting.Components.FiscalPrintoutsEnumCombo"
}; 
this.IDC_ACCPARAMDECL_TELEMATICDECLARATIONTYPE_itemSource = {
  "name": "ExportFileEnumCombo",
  "namespace": "ERP.Accounting.Components.DeclarationExportFileEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['TaxAmount','TAXJournalsReferencesDetails','TelematicDeclarations','CustomizedPrint','AccBookParametersPrint','BalancesExportFiles','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'TaxAmount':['Line','Description','Amount','ToDebit'],'TAXJournalsReferencesDetails':['SaleRetailType','l_TEnhTAXJournalsRefDetail_P3','l_TEnhTAXJournalsRefDetail_P1','TaxCode','l_TEnhTAXJournalsRefDetail_P4','l_TEnhTAXJournalsRefDetail_P2','AccountingTemplate','l_TEnhTAXJournalsRefDetail_P5','TaxableAmountColumn','TaxAmountColumn','Description'],'TelematicDeclarations':['TelematicDeclarationType','DeclarationPath'],'CustomizedPrint':['CodeType','DescriptiveText'],'AccBookParametersPrint':['ReportType','ReportNamespace'],'BalancesExportFiles':['TelematicDeclarationType','DeclarationPath']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAMETERS_ACCOUNTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_ACCOUNTINGComponent, resolver);
    }
} 