import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TS_DELETINGService } from './IDD_TS_DELETING.service';

@Component({
    selector: 'tb-IDD_TS_DELETING',
    templateUrl: './IDD_TS_DELETING.component.html',
    providers: [IDD_TS_DELETINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TS_DELETINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TS_CLEAR_TRNSM_DOCTYPE_itemSource: any;

    constructor(document: IDD_TS_DELETINGService,
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
        this.IDC_TS_CLEAR_TRNSM_DOCTYPE_itemSource = {
  "name": "SalesDocAccEnumCombo",
  "namespace": "ERP.TESANConnector.Components.SalesDocAccEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','bElabPeriodAll','bElabPeriodSel','FromElabPeriod','ToElabPeriod','bDocTypeAll','bDocTypeSel','DocType','bDocDateAll','bDocDateSel','FromDocDate','ToDocDate','bDocNoAll','bDocNoSel','FromDocNo','ToDocNo','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TS_DELETINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TS_DELETINGComponent, resolver);
    }
} 