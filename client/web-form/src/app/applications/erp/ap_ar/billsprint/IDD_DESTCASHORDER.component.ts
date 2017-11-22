import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DESTCASHORDERService } from './IDD_DESTCASHORDER.service';

@Component({
    selector: 'tb-IDD_DESTCASHORDER',
    templateUrl: './IDD_DESTCASHORDER.component.html',
    providers: [IDD_DESTCASHORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DESTCASHORDERComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PRINTCASHORDER_CODETYPE_itemSource: any;

    constructor(document: IDD_DESTCASHORDERService,
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
        this.IDC_PRINTCASHORDER_CODETYPE_itemSource = {
  "name": "TypeEnumCombo",
  "namespace": "ERP.AP_AR.Documents.TypeEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BillsType','AllSel1','SlipSel','SlipNo','AllSel','NoSel','FromNo','ToNo','IgnorePrinted','PresDate','SupportBank','DefPrint','bPrintIBAN','SupporttName','Signature','nCurrentElement','GaugeDescription'],'HKLBank':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DESTCASHORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DESTCASHORDERComponent, resolver);
    }
} 