import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OPERATIONSService } from './IDD_OPERATIONS.service';

@Component({
    selector: 'tb-IDD_OPERATIONS',
    templateUrl: './IDD_OPERATIONS.component.html',
    providers: [IDD_OPERATIONSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_OPERATIONSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource: any;
public IDC_TOOLS_MAN_RTGSTEP_itemSource: any;
public IDC_TOOLS_MAN_ALT_itemSource: any;
public IDC_TOOLS_MAN_RTGSTEP_ALT_RTGSTEP_itemSource: any;

    constructor(document: IDD_OPERATIONSService,
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
        this.IDC_LABOUR_DETAILS_ESTIMATED_LABOUR_TYPE_itemSource = {
  "name": "eLabourTypeEnumCombo",
  "namespace": "ERP.Routing.Components.LabourLabourTypeItemSource"
}; 
this.IDC_TOOLS_MAN_RTGSTEP_itemSource = {
  "name": "RtgStepIntCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.RtgStepToolsComboBox"
}; 
this.IDC_TOOLS_MAN_ALT_itemSource = {
  "name": "AlternateStrCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.AlternateToolsItemSource"
}; 
this.IDC_TOOLS_MAN_RTGSTEP_ALT_RTGSTEP_itemSource = {
  "name": "DNRtgStepIntCombo",
  "namespace": "ERP.ToolsManagement.AddOnsCore.AltRtgStepToolsItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Operations':['Operation','Description','WC','IsWC','HourlyCost','CostsOnQty','TotalTime','HourlyCost','UnitCost','AdditionalCost','CostsOnQty','ProcessingTime','SetupTime','TotalTime','QueueTime','CreationDate','Notes','OperationDescriptionFile','LastModificationDate','Item'],'HKLSelectorWC':['Description'],'HKLItemsByGoodType':['Description'],'global':['__DBTLabour','DBTToolsManagement','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','ToolBmp','FamilyBmp'],'@DBTLabour':['__ePhase','__bIsWorker','__eResourceType','__sResourceCode','__nWorkerID','__sResourceDescription','__eLabourType','__bIsPercent','__nAttendancePerc','__nWorkingTime','__dDate','__nNoOfResources'],'DBTToolsManagement':['RtgStepTypeBmp','RtgStep','Alternate','AltRtgStep','Operation','OperationDescription','Usage','IsFamily','Tool','ProcessingType','ToolType','Fixed','UsageQuantity','UsageTime','ExclusiveUse','Source','SourceTool'],'HKLSelectorTools':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OPERATIONSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OPERATIONSComponent, resolver);
    }
} 