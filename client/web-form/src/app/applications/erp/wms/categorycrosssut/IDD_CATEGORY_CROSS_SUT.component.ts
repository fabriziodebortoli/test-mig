import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CATEGORY_CROSS_SUTService } from './IDD_CATEGORY_CROSS_SUT.service';

@Component({
    selector: 'tb-IDD_CATEGORY_CROSS_SUT',
    templateUrl: './IDD_CATEGORY_CROSS_SUT.component.html',
    providers: [IDD_CATEGORY_CROSS_SUTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CATEGORY_CROSS_SUTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CATEGORY_CROSS_SUTService,
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
		boService.appendToModelStructure({'CategoryCrossSUT':['Category','SUT'],'HKLWMCategory':['Description'],'HKLWMSUnitType':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CATEGORY_CROSS_SUTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CATEGORY_CROSS_SUTComponent, resolver);
    }
} 