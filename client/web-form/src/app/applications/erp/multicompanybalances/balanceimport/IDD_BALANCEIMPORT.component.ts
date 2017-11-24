import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BALANCEIMPORTService } from './IDD_BALANCEIMPORT.service';

@Component({
    selector: 'tb-IDD_BALANCEIMPORT',
    templateUrl: './IDD_BALANCEIMPORT.component.html',
    providers: [IDD_BALANCEIMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BALANCEIMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BALANCEIMPORTService,
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
        
        		this.bo.appendToModelStructure({'global':['OwnedCompany','FileName','OnlyCheck','GenerateAccountsNotInMaster','JEDate','AccrualDate','DocDate','nCurrentElement','GaugeDescription'],'HKLOwnedCompanies':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BALANCEIMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BALANCEIMPORTComponent, resolver);
    }
} 