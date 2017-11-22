import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_PURCHORDService } from './IDD_DELETE_PURCHORD.service';

@Component({
    selector: 'tb-IDD_DELETE_PURCHORD',
    templateUrl: './IDD_DELETE_PURCHORD.component.html',
    providers: [IDD_DELETE_PURCHORDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DELETE_PURCHORDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_PURCHORDService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllInternalNo','InternalNoSel','FromInternalNo','ToInternalNo','AllExternalNo','ExternalNoSel','FromExternalNo','ToExternalNo','AllPayed','NotPayed','Payed','AllAccounting','NotInAccounting','InAccounting','AllCancelled','NotCancelled','Cancelled','AllPrinted','NoPrinted','Printed','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_PURCHORDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_PURCHORDComponent, resolver);
    }
} 