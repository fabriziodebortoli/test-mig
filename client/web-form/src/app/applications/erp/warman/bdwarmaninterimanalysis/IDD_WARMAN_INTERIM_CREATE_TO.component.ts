import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WARMAN_INTERIM_CREATE_TOService } from './IDD_WARMAN_INTERIM_CREATE_TO.service';

@Component({
    selector: 'tb-IDD_WARMAN_INTERIM_CREATE_TO',
    templateUrl: './IDD_WARMAN_INTERIM_CREATE_TO.component.html',
    providers: [IDD_WARMAN_INTERIM_CREATE_TOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WARMAN_INTERIM_CREATE_TOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WARMAN_INTERIM_CREATE_TOService,
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
		boService.appendToModelStructure({'global':['TOItem','TOUoM','TOQuantity','TOLot','SourceZone','SourceBin','DestZone','DestBin','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WARMAN_INTERIM_CREATE_TOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WARMAN_INTERIM_CREATE_TOComponent, resolver);
    }
} 