import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MOBILE_CONFIGURATIONService } from './IDD_MOBILE_CONFIGURATION.service';

@Component({
    selector: 'tb-IDD_MOBILE_CONFIGURATION',
    templateUrl: './IDD_MOBILE_CONFIGURATION.component.html',
    providers: [IDD_MOBILE_CONFIGURATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MOBILE_CONFIGURATIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MOBILE_CONFIGURATION_NAMESPACE_itemSource: any;

    constructor(document: IDD_MOBILE_CONFIGURATIONService,
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
        this.IDC_MOBILE_CONFIGURATION_NAMESPACE_itemSource = {
  "name": "NamespacesCombo",
  "namespace": "ERP.WMSMobile.Services.NamespacesCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Namespace','MobileConfigurationFields','bMoveRight','MobileConfigurationAvailableFields','bMoveLeft'],'MobileConfigurationFields':['l_FieldDescription','l_WidthGraphic','l_FixedField','l_EditField','l_MandatoryField'],'MobileConfigurationAvailableFields':['l_FieldDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MOBILE_CONFIGURATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MOBILE_CONFIGURATIONComponent, resolver);
    }
} 