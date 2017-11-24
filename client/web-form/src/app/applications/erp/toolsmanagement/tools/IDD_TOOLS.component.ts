import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLSService } from './IDD_TOOLS.service';

@Component({
    selector: 'tb-IDD_TOOLS',
    templateUrl: './IDD_TOOLS.component.html',
    providers: [IDD_TOOLSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TOOLSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TOOLSService,
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
        
        		this.bo.appendToModelStructure({'global':['GaugeQty','StatusDescription','ImageStatusTool','ToolsToolFamilies','ToolsHistory','ToolsUsedBy','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'Tools':['Tool','Disabled','Description','Item','ExclusiveUse','Drawing','ImagePath','ImagePath','ToolType','RoundingType','RoundingDigit','BarcodeSegment','OperatingInstructions','Notes','Supplier','PurchaseDate','PurchasePrice','WarrantyExpirationDate','Department','Location','MaintenanceWorker','Brand','Model','ToolSerial','PartNumber','ManufacturingDate','CertificationNumber','TechnicalNotes','UsedQuantity','MaxQuantity','WarningQuantity','ToleranceQuantity','TotalQuantity','UsedTime','MaxTime','WarningTime','ToleranceTime','TotalTime','Reconditioning','MaxReconditioning','Overload','ReconditioningStartDate','ReconditioningDuration','LastInspectionDate','InspectionStartDate','InspectionDuration','NextInspectionDate','InspectionValidityDays','InspectionWarningDays'],'HKLItem':['Description'],'HKLSupplier':['CompanyName'],'HKLWorkers':['WorkerDesc'],'ToolsToolFamilies':['Family','LocExclusive','LocDisabled'],'HKLToolsFamilies':['Description'],'ToolsHistory':['BmpStatus','ActionDate','MaintenanceWorker','Action','ToolStatus','Remarks'],'HKLWorkersHistory':['WorkerDesc'],'ToolsUsedBy':['LocalBmpStatus','MONo','BOM','Usage','RtgStep','Alternate','AltRtgStep','ProcessingType','Operation','WC'],'HKLOperations':['Description'],'HKLWC':['Description'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLSComponent, resolver);
    }
} 