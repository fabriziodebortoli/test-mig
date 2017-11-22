import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SEARCH_ZONE_STRATEGYService } from './IDD_SEARCH_ZONE_STRATEGY.service';

@Component({
    selector: 'tb-IDD_SEARCH_ZONE_STRATEGY',
    templateUrl: './IDD_SEARCH_ZONE_STRATEGY.component.html',
    providers: [IDD_SEARCH_ZONE_STRATEGYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SEARCH_ZONE_STRATEGYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SEARCH_ZONE_STRATEGYService,
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
		boService.appendToModelStructure({'DBTSearchZoneStrategy':['Code','Description','Storage','Notes'],'global':['DBTSearchZoneStrategyDet','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTSearchZoneStrategyDet':['Zone','Priority'],'HKLWMZone':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SEARCH_ZONE_STRATEGYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SEARCH_ZONE_STRATEGYComponent, resolver);
    }
} 