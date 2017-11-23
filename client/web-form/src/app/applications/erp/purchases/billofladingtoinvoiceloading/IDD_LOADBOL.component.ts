import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOADBOLService } from './IDD_LOADBOL.service';

@Component({
    selector: 'tb-IDD_LOADBOL',
    templateUrl: './IDD_LOADBOL.component.html',
    providers: [IDD_LOADBOLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOADBOLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOADBOLService,
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
        
        		this.bo.appendToModelStructure({'global':['BillOfLadingToInvoiceLoading'],'BillOfLadingToInvoiceLoading':['Sel','SupplierDocNo','SupplierDocDate','DocNo','DocumentDate','Currency','Payment','TaxJournal','StubBook','ConfInvRsn']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOADBOLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOADBOLComponent, resolver);
    }
} 