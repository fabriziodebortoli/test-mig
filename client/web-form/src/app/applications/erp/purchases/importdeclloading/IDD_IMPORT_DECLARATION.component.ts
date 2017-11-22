import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMPORT_DECLARATIONService } from './IDD_IMPORT_DECLARATION.service';

@Component({
    selector: 'tb-IDD_IMPORT_DECLARATION',
    templateUrl: './IDD_IMPORT_DECLARATION.component.html',
    providers: [IDD_IMPORT_DECLARATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_IMPORT_DECLARATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMPORT_DECLARATIONService,
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
		boService.appendToModelStructure({'ImportDeclLoading':['Importer','ImporterCode','ExporterCode','ImportDeclarationNo','RegistrationDate','CustomsDate','IntermediationType','CustomsState','DischargePlace','GrossWeight','NetWeight','Appearance','ModeOfTransport'],'HKLImporter':['CompNameCompleteWithTaxNumber'],'HKLExporter':['CompNameCompleteWithTaxNumber'],'HKLGoodsAppearance':['Description'],'HKLTransport':['Description'],'global':['TBRNFeImportDeclarationDetail'],'TBRNFeImportDeclarationDetail':['TBREnhImportDeclDet_Selected','AdditionNumber','SeqAdditionNumber','Item','NCM','UoM','Qty'],'HKLItemsBody':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IMPORT_DECLARATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMPORT_DECLARATIONComponent, resolver);
    }
} 