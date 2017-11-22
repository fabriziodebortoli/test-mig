import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACQUISITION_FROM_DEVICEService } from './IDD_ACQUISITION_FROM_DEVICE.service';

@Component({
    selector: 'tb-IDD_ACQUISITION_FROM_DEVICE',
    templateUrl: './IDD_ACQUISITION_FROM_DEVICE.component.html',
    providers: [IDD_ACQUISITION_FROM_DEVICEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACQUISITION_FROM_DEVICEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DEVICE_COMBO_EXTENSIONS_itemSource: any;

    constructor(document: IDD_ACQUISITION_FROM_DEVICEService,
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
        this.IDC_DEVICE_COMBO_EXTENSIONS_itemSource = {
  "name": "ExtensionsToScanItemSource",
  "namespace": "Extensions.EasyAttachment.TbDMS.ExtensionsToScanItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['sSource','sFile','sExtension','bSeparateFiles','bSplitPage','bSplitBarcode']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACQUISITION_FROM_DEVICEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACQUISITION_FROM_DEVICEComponent, resolver);
    }
} 