import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDECLAR394_SLAVEService } from './IDD_TAXDECLAR394_SLAVE.service';

@Component({
    selector: 'tb-IDD_TAXDECLAR394_SLAVE',
    templateUrl: './IDD_TAXDECLAR394_SLAVE.component.html',
    providers: [IDD_TAXDECLAR394_SLAVEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXDECLAR394_SLAVEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDECLAR394_SLAVEService,
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
		boService.appendToModelStructure({'global':['SLAVE_01','SLAVE_02','SLAVE_03','SLAVE_04','SLAVE_05','SLAVE_06','SLAVE_07','SLAVE_08','SLAVE_09','SLAVE_10','SLAVE_11','SLAVE_12','SLAVE_13','SLAVE_14','SLAVE_15','SLAVE_16','SLAVE_17','SLAVE_18','SLAVE_19','SLAVE_20','SLAVE_21','SLAVE_22','SLAVE_23','SLAVE_24','SLAVE_25','SLAVE_26','SLAVE_27','SLAVE_28','SLAVE_29','SLAVE_30','SLAVE_31','SLAVE_32','SLAVE_33','SLAVE_34','SLAVE_35','SLAVE_36']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDECLAR394_SLAVEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDECLAR394_SLAVEComponent, resolver);
    }
} 