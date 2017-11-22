import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEService } from './IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAME.service';

@Component({
    selector: 'tb-IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAME',
    templateUrl: './IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAME.component.html',
    providers: [IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEService,
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
		boService.appendToModelStructure({'global':['CompleteMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEComponent, resolver);
    }
} 