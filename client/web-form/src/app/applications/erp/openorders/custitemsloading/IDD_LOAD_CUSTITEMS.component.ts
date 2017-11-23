import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_CUSTITEMSService } from './IDD_LOAD_CUSTITEMS.service';

@Component({
    selector: 'tb-IDD_LOAD_CUSTITEMS',
    templateUrl: './IDD_LOAD_CUSTITEMS.component.html',
    providers: [IDD_LOAD_CUSTITEMSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOAD_CUSTITEMSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_CUSTITEMSService,
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
        
        		this.bo.appendToModelStructure({'CustItemsLoading':['CustSupp','CompanyName'],'global':['CustContrLinesLoading'],'CustContrLinesLoading':['Selected','ArtType','Item','ArtDescri']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_CUSTITEMSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_CUSTITEMSComponent, resolver);
    }
} 