import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOADVARIANTService } from './IDD_LOADVARIANT.service';

@Component({
    selector: 'tb-IDD_LOADVARIANT',
    templateUrl: './IDD_LOADVARIANT.component.html',
    providers: [IDD_LOADVARIANTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOADVARIANTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOADVARIANTService,
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
		boService.appendToModelStructure({'global':['ComponentsVariantToCopy','RoutingVariantToCopy'],'ComponentsVariantToCopy':['Sel','SubstType','Component','Description','ComponentVariant','Qty','UoM','FixedComponent','ScrapQty','ScrapUM','ValidityStartingDate','ValidityEndingDate','DNRtgStep'],'RoutingVariantToCopy':['Sel','SubstType','RtgStep','Alternate','AltRtgStep','Operation','WC','SetupTime','ProcessingTime','TotalTime','Quantity','QueueTime']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOADVARIANTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOADVARIANTComponent, resolver);
    }
} 