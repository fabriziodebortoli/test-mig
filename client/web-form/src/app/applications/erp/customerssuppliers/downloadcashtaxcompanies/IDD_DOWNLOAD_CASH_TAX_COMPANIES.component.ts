import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DOWNLOAD_CASH_TAX_COMPANIESService } from './IDD_DOWNLOAD_CASH_TAX_COMPANIES.service';

@Component({
    selector: 'tb-IDD_DOWNLOAD_CASH_TAX_COMPANIES',
    templateUrl: './IDD_DOWNLOAD_CASH_TAX_COMPANIES.component.html',
    providers: [IDD_DOWNLOAD_CASH_TAX_COMPANIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DOWNLOAD_CASH_TAX_COMPANIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DOWNLOAD_CASH_TAX_COMPANIESService,
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
        
        		this.bo.appendToModelStructure({'global':['strFileType','LastUpdateDate','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DOWNLOAD_CASH_TAX_COMPANIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DOWNLOAD_CASH_TAX_COMPANIESComponent, resolver);
    }
} 