import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_ECOService } from './IDD_LOAD_ECO.service';

@Component({
    selector: 'tb-IDD_LOAD_ECO',
    templateUrl: './IDD_LOAD_ECO.component.html',
    providers: [IDD_LOAD_ECOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_ECOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_ECOService,
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
		boService.appendToModelStructure({'global':['sBOMHeader','sBOMHeaderDescription','sBOMHeaderVariant','VariantsLoadingComponentsForEco','VariantsLoadingRouting','BOMLoadingComponentsForEco','BOMLoadingRouting','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'VariantsLoadingComponentsForEco':['Selected','SubstType','ComponentType','Component','Description','Qty','UoM','FixedQty','FixedQty','SetFixedQtyOnMO','ScrapQty','ScrapUM','Variant','ValidityStartingDate','ValidityEndingDate','DNRtgStep','Notes'],'VariantsLoadingRouting':['Selected','SubstType','RtgStep','Alternate','AltRtgStep','Operation','Notes','IsWC','WC','ProcessingTime','TotalTime','Qty','SetupTime','LineTypeInDN'],'BOMLoadingComponentsForEco':['Selected','ComponentType','Component','Qty','UoM','FixedQty','SetFixedQtyOnMO','FixedComponent','Waste','ScrapQty','ScrapUM','Variant','ValidityStartingDate','ValidityEndingDate','DNRtgStep','Notes'],'BOMLoadingRouting':['Selected','RtgStep','Alternate','AltRtgStep','Operation','Description','Notes','IsWC','WC','ProcessingTime','TotalTime','Qty','SetupTime','LineTypeInDN']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_ECOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_ECOComponent, resolver);
    }
} 