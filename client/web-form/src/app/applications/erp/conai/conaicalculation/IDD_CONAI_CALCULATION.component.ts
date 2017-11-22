import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONAI_CALCULATIONService } from './IDD_CONAI_CALCULATION.service';

@Component({
    selector: 'tb-IDD_CONAI_CALCULATION',
    templateUrl: './IDD_CONAI_CALCULATION.component.html',
    providers: [IDD_CONAI_CALCULATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CONAI_CALCULATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CONAI_CALCULATIONService,
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
		boService.appendToModelStructure({'global':['Detail'],'Detail':['Item','Material','PackageType','Description','Qty','ExemptQty','SubjectedQty','UnitValue','TaxableAmount','ExemptionPerc','TaxCode','Offset'],'HKLItems':['Description'],'HKLMaterials':['Description'],'HKLPackageTypes':['PackageTypeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONAI_CALCULATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONAI_CALCULATIONComponent, resolver);
    }
} 