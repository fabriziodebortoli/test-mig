import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOOLS_REGENERATION_SET_DATAService } from './IDD_TOOLS_REGENERATION_SET_DATA.service';

@Component({
    selector: 'tb-IDD_TOOLS_REGENERATION_SET_DATA',
    templateUrl: './IDD_TOOLS_REGENERATION_SET_DATA.component.html',
    providers: [IDD_TOOLS_REGENERATION_SET_DATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TOOLS_REGENERATION_SET_DATAComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MPARSEDCOMBO_itemSource: any;

    constructor(document: IDD_TOOLS_REGENERATION_SET_DATAService,
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
        this.IDC_MPARSEDCOMBO_itemSource = {}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Remarks','Worker','ToolStatus','SetRecDate','nDuration'],'HKLWorkersSetData':['WorkerDesc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOOLS_REGENERATION_SET_DATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOOLS_REGENERATION_SET_DATAComponent, resolver);
    }
} 