import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCOUNTINGASSIGNMENTService } from './IDD_COSTACCOUNTINGASSIGNMENT.service';

@Component({
    selector: 'tb-IDD_COSTACCOUNTINGASSIGNMENT',
    templateUrl: './IDD_COSTACCOUNTINGASSIGNMENT.component.html',
    providers: [IDD_COSTACCOUNTINGASSIGNMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTACCOUNTINGASSIGNMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTACCOUNTINGASSIGNMENTService,
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
		boService.appendToModelStructure({'global':['bAllKind','bSelKind','Nature','AccountType','bAllAccount','bSelAccount','FromRoot','FromRoot','ToRoot','nCurrentElement','GaugeDescription'],'HKLFromAccount':['Description','Description'],'HKLToAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCOUNTINGASSIGNMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCOUNTINGASSIGNMENTComponent, resolver);
    }
} 