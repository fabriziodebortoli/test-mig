import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FACTORIESService } from './IDD_FACTORIES.service';

@Component({
    selector: 'tb-IDD_FACTORIES',
    templateUrl: './IDD_FACTORIES.component.html',
    providers: [IDD_FACTORIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FACTORIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FACTORIESService,
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
		boService.appendToModelStructure({'Factories':['Factory','Description','Notes','PickingStorage','PickingStorageSemifinished','PickingExtStorage','PickingExtStorageSF','DefaultStorage','DefaultSFStorage','DefaultSecondRateStorage','DefaultScrapStorage','DefaultExtStorage','DefaultExtSFStorage','DefaultExtSecondRateStorage','DefaultExtScrapStorage','WasteStorage','WasteDifferentItemStorage'],'HKLPickingStorage':['Description'],'HKLPickingStorageSF':['Description'],'HKLPickingExtStorage':['Description'],'HKLPickingExtStorageSF':['Description'],'HKLStorageFP':['Description'],'HKLSFStorage':['Description'],'HKLSecondRateStorage':['Description'],'HKLScrapStorage':['Description'],'HKLExtStorageFP':['Description'],'HKLExtStorageSF':['Description'],'HKLExtSecondRateStorage':['Description'],'HKLExtScrapStorage':['Description'],'HKLWasteStorage':['Description'],'HKLWasteDifftemStorage':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FACTORIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FACTORIESComponent, resolver);
    }
} 