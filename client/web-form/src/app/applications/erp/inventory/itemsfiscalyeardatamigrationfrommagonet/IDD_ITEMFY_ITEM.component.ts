import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ITEMFY_ITEMService } from './IDD_ITEMFY_ITEM.service';

@Component({
    selector: 'tb-IDD_ITEMFY_ITEM',
    templateUrl: './IDD_ITEMFY_ITEM.component.html',
    providers: [IDD_ITEMFY_ITEMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ITEMFY_ITEMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ITEMFY_ITEMService,
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
		boService.appendToModelStructure({'global':['bAllItems','bItemSel','FromItem','ToItem','bIgnoreNotTransactableItems','bIgnoreDisabledItems','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ITEMFY_ITEMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ITEMFY_ITEMComponent, resolver);
    }
} 