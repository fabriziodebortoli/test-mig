import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXJOURN_RANGEService } from './IDD_TAXJOURN_RANGE.service';

@Component({
    selector: 'tb-IDD_TAXJOURN_RANGE',
    templateUrl: './IDD_TAXJOURN_RANGE.component.html',
    providers: [IDD_TAXJOURN_RANGEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXJOURN_RANGEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXJOURN_RANGEService,
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
		boService.appendToModelStructure({'TaxJournalsRange':['TaxJournal','FirstRangeNo','LastRangeNo','LastAssignedNo'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXJOURN_RANGEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXJOURN_RANGEComponent, resolver);
    }
} 