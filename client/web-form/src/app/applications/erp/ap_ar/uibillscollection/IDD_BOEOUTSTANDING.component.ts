import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOEOUTSTANDINGService } from './IDD_BOEOUTSTANDING.service';

@Component({
    selector: 'tb-IDD_BOEOUTSTANDING',
    templateUrl: './IDD_BOEOUTSTANDING.component.html',
    providers: [IDD_BOEOUTSTANDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOEOUTSTANDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOEOUTSTANDINGService,
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
		boService.appendToModelStructure({'global':['Bank','CA','DueDate','Customer','FiscalNo','PostingDate','DocDate','NrDoc','Charges','Bills','TotalAmount'],'Bills':['l_P1','FiscalNo','Supplier','Customer','l_P2','DueDate','BillType','BillStatus','Amount','l_P5','l_P6','l_P3','Supplier','l_P7','l_P8','l_P4']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOEOUTSTANDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOEOUTSTANDINGComponent, resolver);
    }
} 