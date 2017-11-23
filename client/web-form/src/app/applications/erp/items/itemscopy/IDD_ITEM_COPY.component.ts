import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEM_COPYService } from './IDD_ITEM_COPY.service';

@Component({
    selector: 'tb-IDD_ITEM_COPY',
    templateUrl: './IDD_ITEM_COPY.component.html',
    providers: [IDD_ITEM_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ITEM_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEM_COPYService,
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
        
        		this.bo.appendToModelStructure({'global':['OldItem','NewItem','ItemCustomers','ItemSupp','ExcludeLastInv','ItemBOM','LifoFifoHistoric','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEM_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEM_COPYComponent, resolver);
    }
} 