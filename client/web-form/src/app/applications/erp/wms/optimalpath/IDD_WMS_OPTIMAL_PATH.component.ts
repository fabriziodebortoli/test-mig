import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WMS_OPTIMAL_PATHService } from './IDD_WMS_OPTIMAL_PATH.service';

@Component({
    selector: 'tb-IDD_WMS_OPTIMAL_PATH',
    templateUrl: './IDD_WMS_OPTIMAL_PATH.component.html',
    providers: [IDD_WMS_OPTIMAL_PATHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WMS_OPTIMAL_PATHComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WMS_OPTIMAL_PATHService,
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
		boService.appendToModelStructure({'global':['Storage','bBinPathSequence','bBinStructPathSelect','DBTNodeDetail','LegendStorage','LegendZone','LegendBin'],'DBTNodeDetail':['l_FieldValue','l_FieldName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WMS_OPTIMAL_PATHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WMS_OPTIMAL_PATHComponent, resolver);
    }
} 