import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLS_REGENERATIONService } from './IDD_TOOLS_REGENERATION.service';

@Component({
    selector: 'tb-IDD_TOOLS_REGENERATION',
    templateUrl: './IDD_TOOLS_REGENERATION.component.html',
    providers: [IDD_TOOLS_REGENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TOOLS_REGENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TOOLS_REGENERATION_ACTION_BE_itemSource: any;

    constructor(document: IDD_TOOLS_REGENERATIONService,
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
        this.IDC_TOOLS_REGENERATION_ACTION_BE_itemSource = {
  "name": "ToolStatusEnumCombo",
  "namespace": "ERP.ToolsManagement.Documents.RegenerationStatusItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllTools','bSelTools','FromTool','ToTool','bAllFamilies','bSelFamilies','FromFamily','ToFamily','ToolType','bToBeReconditioned','bUnderMaintenance','ToolsRegeneration'],'ToolsRegeneration':['LocalBmpStatus','Selected','Tool','ActualStatus','UsedQuantity','UsedTime','MaxQuantity','MaxTime','Reconditioning','MaxReconditioning','LocalRemarks','MaintenanceWorker','ReconditioningStartDate','ReconditioningDuration','ToolStatus'],'HKLToolsBE':['Description'],'HKLWorkers':['WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLS_REGENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLS_REGENERATIONComponent, resolver);
    }
} 