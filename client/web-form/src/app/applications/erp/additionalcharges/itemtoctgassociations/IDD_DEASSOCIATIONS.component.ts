import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEASSOCIATIONSService } from './IDD_DEASSOCIATIONS.service';

@Component({
    selector: 'tb-IDD_DEASSOCIATIONS',
    templateUrl: './IDD_DEASSOCIATIONS.component.html',
    providers: [IDD_DEASSOCIATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DEASSOCIATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEASSOCIATIONSService,
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
		boService.appendToModelStructure({'ItemToCtgAssociations':['Item','DistributionTemplate','ChargeCategory'],'HKLItem':['Description'],'HKLDistribTemplates':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEASSOCIATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEASSOCIATIONSComponent, resolver);
    }
} 