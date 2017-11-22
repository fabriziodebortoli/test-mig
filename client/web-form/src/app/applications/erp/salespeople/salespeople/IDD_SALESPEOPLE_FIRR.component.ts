import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALESPEOPLE_FIRRService } from './IDD_SALESPEOPLE_FIRR.service';

@Component({
    selector: 'tb-IDD_SALESPEOPLE_FIRR',
    templateUrl: './IDD_SALESPEOPLE_FIRR.component.html',
    providers: [IDD_SALESPEOPLE_FIRRService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALESPEOPLE_FIRRComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALESPEOPLE_FIRRService,
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
		boService.appendToModelStructure({'FIRR':['IsManual','BalanceYear','CodeType','Base','AccruedAmount','PaymentDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALESPEOPLE_FIRRFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALESPEOPLE_FIRRComponent, resolver);
    }
} 