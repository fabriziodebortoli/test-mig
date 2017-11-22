import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOMIMPLOSIONService } from './IDD_BOMIMPLOSION.service';

@Component({
    selector: 'tb-IDD_BOMIMPLOSION',
    templateUrl: './IDD_BOMIMPLOSION.component.html',
    providers: [IDD_BOMIMPLOSIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOMIMPLOSIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_IMPLOS_CODETYPE_COMP_itemSource: any;

    constructor(document: IDD_BOMIMPLOSIONService,
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
        this.IDC_IMPLOS_CODETYPE_COMP_itemSource = {
  "name": "{{RuntimeClassCodeType}}",
  "namespace": "{{ItemSourcesNamespace}}"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ComponentType','Component','bNotExplodeVariant','bVariantSelAll','bVariantSel','Variant','bLevelsSelAll','bLevelSel','NrLevel','AllDate','SelDate','Date','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOMIMPLOSIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOMIMPLOSIONComponent, resolver);
    }
} 