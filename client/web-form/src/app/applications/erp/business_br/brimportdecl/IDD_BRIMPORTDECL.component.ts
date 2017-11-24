import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BRIMPORTDECLService } from './IDD_BRIMPORTDECL.service';

@Component({
    selector: 'tb-IDD_BRIMPORTDECL',
    templateUrl: './IDD_BRIMPORTDECL.component.html',
    providers: [IDD_BRIMPORTDECLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BRIMPORTDECLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BRIMPORTDECLService,
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
        
        		this.bo.appendToModelStructure({'DBTBRImportDecl':['Importer','ImporterCode','ExporterCode','ImportDeclarationNo','RegistrationDate','CustomsDate','IntermediationType','CustomsState','DischargePlace','InNotaFiscal','GrossWeight','NetWeight','Appearance','ModeOfTransport'],'HKLImporter':['CompNameCompleteWithTaxNumber'],'HKLExporter':['CompNameCompleteWithTaxNumber'],'HKLGoodsAppearance':['Description'],'HKLTransport':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BRIMPORTDECLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BRIMPORTDECLComponent, resolver);
    }
} 