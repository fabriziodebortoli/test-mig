import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONTROL_PANEL_FINDService } from './IDD_CONTROL_PANEL_FIND.service';

@Component({
    selector: 'tb-IDD_CONTROL_PANEL_FIND',
    templateUrl: './IDD_CONTROL_PANEL_FIND.component.html',
    providers: [IDD_CONTROL_PANEL_FINDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CONTROL_PANEL_FINDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CONTROL_PANEL_FIND_TYPE_itemSource: any;

    constructor(document: IDD_CONTROL_PANEL_FINDService,
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
        this.IDC_CONTROL_PANEL_FIND_TYPE_itemSource = {
  "name": "FindTypeStrCombo",
  "namespace": "ERP.ManufacturingPlus.Documents.ControlPanelFindItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FindType','FindValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONTROL_PANEL_FINDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONTROL_PANEL_FINDComponent, resolver);
    }
} 