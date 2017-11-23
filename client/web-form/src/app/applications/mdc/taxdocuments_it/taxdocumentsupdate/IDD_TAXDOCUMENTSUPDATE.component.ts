import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDOCUMENTSUPDATEService } from './IDD_TAXDOCUMENTSUPDATE.service';

@Component({
    selector: 'tb-IDD_TAXDOCUMENTSUPDATE',
    templateUrl: './IDD_TAXDOCUMENTSUPDATE.component.html',
    providers: [IDD_TAXDOCUMENTSUPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDOCUMENTSUPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDOCUMENTSUPDATEService,
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
		boService.appendToModelStructure({'global':['FromSetupDate','ToSetupDate','FromSendingDate','ToSendingDate','bAllSendingNo','bSelSendingNo','FromSendingNo','ToSendingNo','bAllSendingTypes','bSelSendingTypes','SendingType','EnhTaxDocSendings'],'EnhTaxDocSendings':['TaxDocSendingNo','SendingDate','SendingType','SetupDate','FromDate','ToDate','SendingStatus','l_TTaxDocumentsSendingDetail_P02','l_TTaxDocumentsSendingDetail_P03','l_TTaxDocumentsSendingDetail_P04','IDFile']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDOCUMENTSUPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDOCUMENTSUPDATEComponent, resolver);
    }
} 