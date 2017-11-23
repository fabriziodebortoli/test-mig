import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDOCUMENTSSENDINGService } from './IDD_TAXDOCUMENTSSENDING.service';

@Component({
    selector: 'tb-IDD_TAXDOCUMENTSSENDING',
    templateUrl: './IDD_TAXDOCUMENTSSENDING.component.html',
    providers: [IDD_TAXDOCUMENTSSENDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDOCUMENTSSENDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDOCUMENTSSENDINGService,
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
		boService.appendToModelStructure({'global':['FromDate','ToDate','bAllSendingNo','bSelSendingNo','FromSendingNo','ToSendingNo','bAllSendingTypes','bSelSendingTypes','SendingType','EnhTaxDocSendings'],'EnhTaxDocSendings':['l_TEnhTaxDocSendings_P1','TaxDocSendingNo','SendingType','SetupDate','FromDate','ToDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDOCUMENTSSENDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDOCUMENTSSENDINGComponent, resolver);
    }
} 