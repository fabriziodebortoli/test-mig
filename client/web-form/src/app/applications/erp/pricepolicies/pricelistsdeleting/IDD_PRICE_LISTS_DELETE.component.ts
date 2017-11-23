import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRICE_LISTS_DELETEService } from './IDD_PRICE_LISTS_DELETE.service';

@Component({
    selector: 'tb-IDD_PRICE_LISTS_DELETE',
    templateUrl: './IDD_PRICE_LISTS_DELETE.component.html',
    providers: [IDD_PRICE_LISTS_DELETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRICE_LISTS_DELETEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRICE_LISTS_DELETEService,
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
		boService.appendToModelStructure({'global':['bAllPriceList','bPriceListSel','FromPriceList','ToPriceList','bAllEdition','bNotValid','NotValidDate','bValid','DateValidFrom','PriceListsDeleting'],'PriceListsDeleting':['PriceLists_Selected','PriceList','Description','PriceLists_ValidityStartingDate','PriceLists_ValidityEndingDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRICE_LISTS_DELETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRICE_LISTS_DELETEComponent, resolver);
    }
} 