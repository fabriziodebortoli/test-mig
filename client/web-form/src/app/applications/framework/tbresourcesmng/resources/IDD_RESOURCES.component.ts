import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RESOURCESService } from './IDD_RESOURCES.service';

@Component({
    selector: 'tb-IDD_RESOURCES',
    templateUrl: './IDD_RESOURCES.component.html',
    providers: [IDD_RESOURCESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_RESOURCESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RESOURCESService,
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
        
        		this.bo.appendToModelStructure({'Resources':['ResourceType','Disabled','ResourceCode','Description','ImagePath','ImagePath','DomicilyAddress','Address2','DomicilyCity','DomicilyZip','DomicilyCounty','DomicilyCountry','Latitude','Longitude','Telephone4','Telephone2','Telephone3','Telephone1','SkypeID','Email','URL','Notes','Manager','ManagerDesc','HideOnLayout'],'global':['ResourcesAbsences','ResourcesFields','ResourcesDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ResourcesAbsences':['Reason','StartingDate','EndingDate','Manager','ManagerDesc','Notes'],'ResourcesFields':['FieldName','FieldValue','HideOnLayout','Notes'],'ResourcesDetails':['IsWorker','ChildResourceType','ChildResourceCode','ChildWorkerID','WorkerDesc','ManagerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RESOURCESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RESOURCESComponent, resolver);
    }
} 