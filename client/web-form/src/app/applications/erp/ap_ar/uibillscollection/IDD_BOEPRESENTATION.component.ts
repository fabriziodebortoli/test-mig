import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOEPRESENTATIONService } from './IDD_BOEPRESENTATION.service';

@Component({
    selector: 'tb-IDD_BOEPRESENTATION',
    templateUrl: './IDD_BOEPRESENTATION.component.html',
    providers: [IDD_BOEPRESENTATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOEPRESENTATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOEPRESENTATIONService,
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
		boService.appendToModelStructure({'global':['DueDate','Customer','FiscalNo','AllCustomer','SelCustomer','FromCustomer','ToCustomer','OrderByDate','OrderByCustomer','Bank','CA','Bank','CA','PresentationDate','PostingDate','DocDate','NrDoc','Charges','Bills','TotalAmount'],'Bills':['l_P1','FiscalNo','Supplier','Customer','l_P2','DueDate','BillType','BillStatus','Amount','l_P5','l_P6','l_P3','Supplier','l_P7','l_P8','l_P4']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOEPRESENTATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOEPRESENTATIONComponent, resolver);
    }
} 