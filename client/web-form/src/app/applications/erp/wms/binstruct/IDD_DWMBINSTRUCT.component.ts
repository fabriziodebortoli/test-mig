import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DWMBINSTRUCTService } from './IDD_DWMBINSTRUCT.service';

@Component({
    selector: 'tb-IDD_DWMBINSTRUCT',
    templateUrl: './IDD_DWMBINSTRUCT.component.html',
    providers: [IDD_DWMBINSTRUCTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DWMBINSTRUCTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DWMBINSTRUCT_SEPARATOR_itemSource: any;

    constructor(document: IDD_DWMBINSTRUCTService,
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
        this.IDC_DWMBINSTRUCT_SEPARATOR_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.WMS.Separators"
}; 

        		this.bo.appendToModelStructure({'DBTWMBinStruct':['BinStructure','Description','Separator'],'global':['Structure','DBTWMBinStructDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DWMBINSTRUCTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DWMBINSTRUCTComponent, resolver);
    }
} 