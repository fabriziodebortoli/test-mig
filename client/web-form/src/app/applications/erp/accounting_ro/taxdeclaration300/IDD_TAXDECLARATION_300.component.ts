import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDECLARATION_300Service } from './IDD_TAXDECLARATION_300.service';

@Component({
    selector: 'tb-IDD_TAXDECLARATION_300',
    templateUrl: './IDD_TAXDECLARATION_300.component.html',
    providers: [IDD_TAXDECLARATION_300Service, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDECLARATION_300Component extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDECLARATION_300Service,
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
		boService.appendToModelStructure({'global':['DeclType','Year','Period','DisabledFilter','Name','Surname','Function','BankCode','cont','PaymentDate','nr_evid','bifa_interne','temei','PrintFile','PrintPaper','ProcessStatus']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDECLARATION_300FactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDECLARATION_300Component, resolver);
    }
} 