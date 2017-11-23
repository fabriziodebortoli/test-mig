import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MASSIVE_VALIDATIONService } from './IDD_MASSIVE_VALIDATION.service';

@Component({
    selector: 'tb-IDD_MASSIVE_VALIDATION',
    templateUrl: './IDD_MASSIVE_VALIDATION.component.html',
    providers: [IDD_MASSIVE_VALIDATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MASSIVE_VALIDATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MASSIVE_VALIDATIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bCheckItems','bCheckCustomers','bCheckSuppliers','bCheckContacts','bCheckProspSupp','bAllCustomes','bSelCustomes','FromCustomer','ToCustomer','bAllCustCtg','bSelCustCtg','FromCustCtg','ToCustCtg','bAllContacts','bSelContacts','FromContact','ToContact','bAllProspSupp','bSelProspSupp','FromProspSupp','ToProspSupp','bAllSuppliers','bSelSuppliers','FromSupplier','ToSupplier','bAllSuppCtg','bSelSuppCtg','FromSuppCtg','ToSuppCtg','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MASSIVE_VALIDATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MASSIVE_VALIDATIONComponent, resolver);
    }
} 