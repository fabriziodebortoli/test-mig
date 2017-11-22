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
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_DWMBINSTRUCT_SEPARATOR_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.WMS.Separators"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTWMBinStruct':['BinStructure','Description','Separator'],'global':['Structure','DBTWMBinStructDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

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