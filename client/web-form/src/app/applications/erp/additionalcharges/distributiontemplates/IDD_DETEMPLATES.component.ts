import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DETEMPLATESService } from './IDD_DETEMPLATES.service';

@Component({
    selector: 'tb-IDD_DETEMPLATES',
    templateUrl: './IDD_DETEMPLATES.component.html',
    providers: [IDD_DETEMPLATESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DETEMPLATESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DETEMPLATESService,
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
		boService.appendToModelStructure({'DistributionTemplates':['DistributionTemplate','Description','DistributionBase','InvRsn','InvRsnNeg'],'HKLInvRsn':['Description'],'HKLInvRsnNeg':['Description'],'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['ChargeCategory','Description','ChargePerc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DETEMPLATESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DETEMPLATESComponent, resolver);
    }
} 