import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOTSService } from './IDD_LOTS.service';

@Component({
    selector: 'tb-IDD_LOTS',
    templateUrl: './IDD_LOTS.component.html',
    providers: [IDD_LOTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOTSService,
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
		boService.appendToModelStructure({'Items':['Item','Description'],'GoodsData':['UseLots','TraceabilityCritical','UseSupplierLotAsNewLotNumber','LotPreexpiringDays','LotValidityDays'],'global':['Lots','ValidLotNo','DisabledLotNo','NotExpiredLotNo','TotallyConsumedLotNo','ExpiredLotNo','Numbering','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'Lots':['PurchaseDate','DescriptionText'],'Numbering':['P1','LastLotNo'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOTSComponent, resolver);
    }
} 