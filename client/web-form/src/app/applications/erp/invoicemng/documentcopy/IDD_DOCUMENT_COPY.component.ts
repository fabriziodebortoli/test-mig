import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DOCUMENT_COPYService } from './IDD_DOCUMENT_COPY.service';

@Component({
    selector: 'tb-IDD_DOCUMENT_COPY',
    templateUrl: './IDD_DOCUMENT_COPY.component.html',
    providers: [IDD_DOCUMENT_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DOCUMENT_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INVOICE_DOC_TAB_COPY_DOC_CODETYPE_itemSource: any;

    constructor(document: IDD_DOCUMENT_COPYService,
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
        this.IDC_INVOICE_DOC_TAB_COPY_DOC_CODETYPE_itemSource = {
  "name": "CopyDocTypeCombo",
  "namespace": "{{sNSCopyOn}}"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CopyDoc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DOCUMENT_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DOCUMENT_COPYComponent, resolver);
    }
} 