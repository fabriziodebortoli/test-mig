import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BANKSDOWNLOADERService } from './IDD_BANKSDOWNLOADER.service';

@Component({
    selector: 'tb-IDD_BANKSDOWNLOADER',
    templateUrl: './IDD_BANKSDOWNLOADER.component.html',
    providers: [IDD_BANKSDOWNLOADERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BANKSDOWNLOADERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BANKSDOWNLOADERService,
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
		boService.appendToModelStructure({'global':['FromABI','ToABI','FromCAB','ToCAB','DownloadType','UpdateDate','bNewBanksFromUpdateFile','bCancelledBanks','bUpdateBanks','bAbsorbedBanks','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BANKSDOWNLOADERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BANKSDOWNLOADERComponent, resolver);
    }
} 