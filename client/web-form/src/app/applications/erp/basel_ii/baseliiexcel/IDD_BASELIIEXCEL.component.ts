import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BASELIIEXCELService } from './IDD_BASELIIEXCEL.service';

@Component({
    selector: 'tb-IDD_BASELIIEXCEL',
    templateUrl: './IDD_BASELIIEXCEL.component.html',
    providers: [IDD_BASELIIEXCELService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BASELIIEXCELComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BASELIIEXCEL_CALCSHEETID_itemSource: any;

    constructor(document: IDD_BASELIIEXCELService,
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
        this.IDC_BASELIIEXCEL_CALCSHEETID_itemSource = {
  "name": "CalcSheetMap",
  "namespace": "ERP.Basel_II.Documents.CalcSheetMapItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FiscalYear','Month','bRounding','CalcSheetId','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BASELIIEXCELFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BASELIIEXCELComponent, resolver);
    }
} 