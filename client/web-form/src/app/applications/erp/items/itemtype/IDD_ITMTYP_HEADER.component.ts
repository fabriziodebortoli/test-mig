import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITMTYP_HEADERService } from './IDD_ITMTYP_HEADER.service';

@Component({
    selector: 'tb-IDD_ITMTYP_HEADER',
    templateUrl: './IDD_ITMTYP_HEADER.component.html',
    providers: [IDD_ITMTYP_HEADERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITMTYP_HEADERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITMTYP_HEADERService,
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
        
        		this.bo.appendToModelStructure({'ItemType':['CodeType','Description','DiscountFormula','HasCustomers','HasSuppliers','Notes'],'global':['ItemTypeBudget','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITMTYP_HEADERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITMTYP_HEADERComponent, resolver);
    }
} 