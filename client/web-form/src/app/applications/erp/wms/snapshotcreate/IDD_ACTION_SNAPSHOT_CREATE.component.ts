import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACTION_SNAPSHOT_CREATEService } from './IDD_ACTION_SNAPSHOT_CREATE.service';

@Component({
    selector: 'tb-IDD_ACTION_SNAPSHOT_CREATE',
    templateUrl: './IDD_ACTION_SNAPSHOT_CREATE.component.html',
    providers: [IDD_ACTION_SNAPSHOT_CREATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACTION_SNAPSHOT_CREATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACTION_SNAPSHOT_CREATEService,
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
		boService.appendToModelStructure({'global':['bAllStorage','bSelectionStorage','SelectedStorage','bAllZone','bSelectionZone','SelectedZone','bAllBins','bSelectBins','BinFrom','BinTo','bAllItems','bSelectItems','ItemFrom','ItemTo','bCertify','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACTION_SNAPSHOT_CREATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACTION_SNAPSHOT_CREATEComponent, resolver);
    }
} 