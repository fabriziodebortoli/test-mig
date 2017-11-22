import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ROOT_SMARTCODService } from './IDD_ROOT_SMARTCOD.service';

@Component({
    selector: 'tb-IDD_ROOT_SMARTCOD',
    templateUrl: './IDD_ROOT_SMARTCOD.component.html',
    providers: [IDD_ROOT_SMARTCODService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ROOT_SMARTCODComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ROOT_SMARTCODService,
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
		boService.appendToModelStructure({'Root':['Root','Disabled','Description','BaseUoM','TaxCode','CommodityCtg','HomogeneousCtg','CalculatePrice','Price','CalculateWeight','Weight','SaleOffset','PurchaseOffset','Notes','FactorNo','FactorValue','FactorDescription','GenerateComparableUoM','SetConversionFactor','ComparableUoM','ReferenceUoMCalculatedValue','ConvertCoefficientsInUoM','Length','SeparatorCode','SegmentSeparator','NoOfSegments','DescriptionDelimiter'],'HKLTaxCode':['Description'],'HKLCommodityCtg':['Description'],'HKLHomogeneousCtg':['Description'],'HKLOffsetFixAssets':['Description'],'HKLOffsetPurchase':['Description'],'global':['Language','DescriptionSeparator','Structure','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ROOT_SMARTCODFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ROOT_SMARTCODComponent, resolver);
    }
} 