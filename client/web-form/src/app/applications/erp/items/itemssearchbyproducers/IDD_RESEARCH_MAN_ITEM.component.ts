import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RESEARCH_MAN_ITEMService } from './IDD_RESEARCH_MAN_ITEM.service';

@Component({
    selector: 'tb-IDD_RESEARCH_MAN_ITEM',
    templateUrl: './IDD_RESEARCH_MAN_ITEM.component.html',
    providers: [IDD_RESEARCH_MAN_ITEMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RESEARCH_MAN_ITEMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RESEARCH_MAN_ITEMService,
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
		boService.appendToModelStructure({'global':['ItemsSearchbProductSearchByManufacturer','ItemsSearchbSearchByCompanyName','ItemsSearchbProductSearchByProdCtg','ItemsSearchbSyncWithKeyboard','ItemsSearchStrItem']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RESEARCH_MAN_ITEMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RESEARCH_MAN_ITEMComponent, resolver);
    }
} 