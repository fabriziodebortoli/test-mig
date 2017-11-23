import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_BOM_VARIANTSService } from './IDD_LOAD_BOM_VARIANTS.service';

@Component({
    selector: 'tb-IDD_LOAD_BOM_VARIANTS',
    templateUrl: './IDD_LOAD_BOM_VARIANTS.component.html',
    providers: [IDD_LOAD_BOM_VARIANTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOAD_BOM_VARIANTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_BOM_VARIANTSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BOMLoadingComponents','BOMLoadingRouting'],'BOMLoadingComponents':['Selected','Line','ComponentType','Component','Description','Qty','FixedQty','SetFixedQtyOnMO','UoM','FixedComponent','Valorize','ScrapQty','ScrapUM','Variant','ValidityStartingDate','ValidityEndingDate','DNRtgStep','Notes'],'BOMLoadingRouting':['Selected','RtgStep','Alternate','AltRtgStep','Operation','Notes','WC','ProcessingTime','Qty','LineTypeInDN']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_BOM_VARIANTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_BOM_VARIANTSComponent, resolver);
    }
} 