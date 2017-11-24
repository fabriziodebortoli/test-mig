import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRANSFER_ORDER_LOADINGService } from './IDD_TRANSFER_ORDER_LOADING.service';

@Component({
    selector: 'tb-IDD_TRANSFER_ORDER_LOADING',
    templateUrl: './IDD_TRANSFER_ORDER_LOADING.component.html',
    providers: [IDD_TRANSFER_ORDER_LOADINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TRANSFER_ORDER_LOADINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TRANSFER_ORDER_LOADINGService,
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
        
        		this.bo.appendToModelStructure({'global':['WTTransferOrderTOGRLoading'],'WTTransferOrderTOGRLoading':['Sel','CustSupp','ConsignmentPartner','CustSupp','CompanyName','BillOfLadingNumber','BillOfLadingDate','BillOfLadingNumber','BillOfLadingDate','ReferenceDocType','ReferenceDocNo','ToResource','WorkerName','MacAddress']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRANSFER_ORDER_LOADINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRANSFER_ORDER_LOADINGComponent, resolver);
    }
} 