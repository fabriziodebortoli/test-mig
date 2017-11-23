import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GOODSREC_GENERATEService } from './IDD_GOODSREC_GENERATE.service';

@Component({
    selector: 'tb-IDD_GOODSREC_GENERATE',
    templateUrl: './IDD_GOODSREC_GENERATE.component.html',
    providers: [IDD_GOODSREC_GENERATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_GOODSREC_GENERATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GOODSREC_GENERATEService,
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
		boService.appendToModelStructure({'global':['SUToGenerate','StorageUnitType','bPrintLabelSU','nCurrentElement','GaugeDescription'],'HKLWMStorageUnitType':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GOODSREC_GENERATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GOODSREC_GENERATEComponent, resolver);
    }
} 