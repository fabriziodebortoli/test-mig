import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOL_INSPECTIONService } from './IDD_TOOL_INSPECTION.service';

@Component({
    selector: 'tb-IDD_TOOL_INSPECTION',
    templateUrl: './IDD_TOOL_INSPECTION.component.html',
    providers: [IDD_TOOL_INSPECTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TOOL_INSPECTIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TOOLS_INSPECTIONS_ACTION_itemSource: any;

    constructor(document: IDD_TOOL_INSPECTIONService,
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
        this.IDC_TOOLS_INSPECTIONS_ACTION_itemSource = {
  "name": "ToolStatusEnumCombo",
  "namespace": "ERP.ToolsManagement.Documents.InspectionStatusItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllTools','bSelTools','FromTool','ToTool','bAllFamilies','bSelFamilies','FromFamily','ToFamily','bToInspect','bUnderInspection','ToolsInspection'],'ToolsInspection':['LocalBmpStatus','Selected','Tool','LastInspectionDate','LocalRemarks','MaintenanceWorker','InspectionStartDate','InspectionDuration','InspectionDuration','InspectionValidityDays','NextInspectionDate','ToolStatus'],'HKLToolsBE':['Description'],'HKLWorkers':['WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOL_INSPECTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOL_INSPECTIONComponent, resolver);
    }
} 