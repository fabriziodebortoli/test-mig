import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLS_FAMILIESService } from './IDD_TOOLS_FAMILIES.service';

@Component({
    selector: 'tb-IDD_TOOLS_FAMILIES',
    templateUrl: './IDD_TOOLS_FAMILIES.component.html',
    providers: [IDD_TOOLS_FAMILIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TOOLS_FAMILIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TOOLS_FAMILIESService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['GaugeQty','StatusDescription','ImageStatusTool','ToolsFamiliesDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ToolsFamilies':['Family','Description','FamilyType','Disabled','ExclusiveUse'],'ToolsFamiliesDetails':['LocalBmpStatus','Priority','Tool','LocalToolStatus'],'HKLTools':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLS_FAMILIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLS_FAMILIESComponent, resolver);
    }
} 