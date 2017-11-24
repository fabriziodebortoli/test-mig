import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MO_LISTService } from './IDD_MO_LIST.service';

@Component({
    selector: 'tb-IDD_MO_LIST',
    templateUrl: './IDD_MO_LIST.component.html',
    providers: [IDD_MO_LISTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MO_LISTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MO_LISTService,
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
        
        		this.bo.appendToModelStructure({'global':['MOComponentsToPick'],'MOComponentsToPick':['MOSelected','Line','Item','LineDescri','UoM','BoLMOQty','BoLMONeededQty','BoLMOProgQty','BoLMOProgNeededQty','Qty','BoLMORemainingQty','BoLMORemainingNeededQty','Lot','MONo','DeliveryDateForMO','Job','DocNo','BoLMOBmpOutsourced']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MO_LISTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MO_LISTComponent, resolver);
    }
} 