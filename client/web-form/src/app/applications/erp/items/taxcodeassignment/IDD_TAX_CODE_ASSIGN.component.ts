import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAX_CODE_ASSIGNService } from './IDD_TAX_CODE_ASSIGN.service';

@Component({
    selector: 'tb-IDD_TAX_CODE_ASSIGN',
    templateUrl: './IDD_TAX_CODE_ASSIGN.component.html',
    providers: [IDD_TAX_CODE_ASSIGNService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAX_CODE_ASSIGNComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAX_CODE_ASSIGNService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','NewTaxCode','OldTaxCode','nCurrentElement','GaugeDescription'],'HKLTaxCode':['Description'],'HKLOldTaxCode':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAX_CODE_ASSIGNFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAX_CODE_ASSIGNComponent, resolver);
    }
} 