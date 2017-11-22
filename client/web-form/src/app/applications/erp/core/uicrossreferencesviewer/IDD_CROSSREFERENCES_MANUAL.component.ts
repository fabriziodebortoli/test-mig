import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CROSSREFERENCES_MANUALService } from './IDD_CROSSREFERENCES_MANUAL.service';

@Component({
    selector: 'tb-IDD_CROSSREFERENCES_MANUAL',
    templateUrl: './IDD_CROSSREFERENCES_MANUAL.component.html',
    providers: [IDD_CROSSREFERENCES_MANUALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CROSSREFERENCES_MANUALComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CROSSREFERENCES_DOCUMENT_itemSource: any;

    constructor(document: IDD_CROSSREFERENCES_MANUALService,
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
        this.IDC_CROSSREFERENCES_DOCUMENT_itemSource = {
  "name": "CrossReferencesEnumCombo",
  "namespace": "ERP.Core.Components.CrossReferencesEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CRViewerManualDocumentType','CRViewerHKLDoc','CRViewerNoteForEdit']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CROSSREFERENCES_MANUALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CROSSREFERENCES_MANUALComponent, resolver);
    }
} 