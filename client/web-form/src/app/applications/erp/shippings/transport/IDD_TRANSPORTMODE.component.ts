import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRANSPORTMODEService } from './IDD_TRANSPORTMODE.service';

@Component({
    selector: 'tb-IDD_TRANSPORTMODE',
    templateUrl: './IDD_TRANSPORTMODE.component.html',
    providers: [IDD_TRANSPORTMODEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TRANSPORTMODEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TRANSPORTMODE_BR_SHIPPING_MODE_itemSource: any;

    constructor(document: IDD_TRANSPORTMODEService,
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
        this.IDC_TRANSPORTMODE_BR_SHIPPING_MODE_itemSource = {
  "name": "BRShippingModeComboBox",
  "namespace": "ERP.Shippings.Documents.BRShippingModeComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Transports':['ModeOfTransport','Description','ShippingType','CodeType','Notes','BRShippingMode','BRShippingModeDescri','ExcludeCharges'],'global':['__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRANSPORTMODEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRANSPORTMODEComponent, resolver);
    }
} 