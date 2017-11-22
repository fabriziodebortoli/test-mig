import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRICELISTS_FULLService } from './IDD_PRICELISTS_FULL.service';

@Component({
    selector: 'tb-IDD_PRICELISTS_FULL',
    templateUrl: './IDD_PRICELISTS_FULL.component.html',
    providers: [IDD_PRICELISTS_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PRICELISTS_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRICELISTS_FULLService,
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
		boService.appendToModelStructure({'PriceLists':['PriceList','Disabled','Description','AlwaysShow','Currency','ValidityStartingDate','ValidityEndingDate','LastModificationDate'],'HKLCurrencies':['Description'],'global':['__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRICELISTS_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRICELISTS_FULLComponent, resolver);
    }
} 