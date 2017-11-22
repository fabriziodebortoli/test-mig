import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDECLAR394Service } from './IDD_TAXDECLAR394.service';

@Component({
    selector: 'tb-IDD_TAXDECLAR394',
    templateUrl: './IDD_TAXDECLAR394.component.html',
    providers: [IDD_TAXDECLAR394Service, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDECLAR394Component extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDECLAR394Service,
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
		boService.appendToModelStructure({'global':['DeclType','Year','Period','bOperation','VATRefound','Function','bSPLegalEntity','bSPNaturalPerson','SPFiscalCode','SPName','SPFunction','SPOtherFunction','bOption','bOption2','NrAMEF','PrintFile','PrintPaper']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDECLAR394FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDECLAR394Component, resolver);
    }
} 