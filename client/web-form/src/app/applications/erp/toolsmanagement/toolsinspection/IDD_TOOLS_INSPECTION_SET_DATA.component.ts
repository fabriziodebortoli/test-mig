import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLS_INSPECTION_SET_DATAService } from './IDD_TOOLS_INSPECTION_SET_DATA.service';

@Component({
    selector: 'tb-IDD_TOOLS_INSPECTION_SET_DATA',
    templateUrl: './IDD_TOOLS_INSPECTION_SET_DATA.component.html',
    providers: [IDD_TOOLS_INSPECTION_SET_DATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TOOLS_INSPECTION_SET_DATAComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MPARSEDCOMBO_itemSource: any;

    constructor(document: IDD_TOOLS_INSPECTION_SET_DATAService,
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
        this.IDC_MPARSEDCOMBO_itemSource = {
  "name": "ToolStatusEnumCombo",
  "namespace": "ERP.ToolsManagement.Documents.InspectionStatusItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Remarks','SetInspDate','Worker','nDuration','ToolStatus'],'HKLWorkersSetData':['WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLS_INSPECTION_SET_DATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLS_INSPECTION_SET_DATAComponent, resolver);
    }
} 