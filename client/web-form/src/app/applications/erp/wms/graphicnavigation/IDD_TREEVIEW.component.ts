import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TREEVIEWService } from './IDD_TREEVIEW.service';

@Component({
    selector: 'tb-IDD_TREEVIEW',
    templateUrl: './IDD_TREEVIEW.component.html',
    providers: [IDD_TREEVIEWService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TREEVIEWComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TREEVIEWService,
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
		boService.appendToModelStructure({'global':['Storage','Zone','bAll','bSelect','BinFrom','BinTo','Item','DBTNodeDetail','LegendStorage','LegendZone','LegendSection','LegendBin','LegendNotEmptyBin','LegendStockWithoutSU','LegendStockWithSU','LegendIsMultilevelSU','LegendSnapshots','LegendDisabled','LegendSuspect','LegendUseSection','LegendBlocked'],'HKLItem':['Description'],'DBTNodeDetail':['l_FieldValue','l_FieldName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TREEVIEWFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TREEVIEWComponent, resolver);
    }
} 