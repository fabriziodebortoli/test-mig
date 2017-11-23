import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WEBSERVICESLISTService } from './IDD_WEBSERVICESLIST.service';

@Component({
    selector: 'tb-IDD_WEBSERVICESLIST',
    templateUrl: './IDD_WEBSERVICESLIST.component.html',
    providers: [IDD_WEBSERVICESLISTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WEBSERVICESLISTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_WEBSERVICES_BODYEDIT_NAMESPACE_COL_itemSource: any;

    constructor(document: IDD_WEBSERVICESLISTService,
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
        this.IDC_WEBSERVICES_BODYEDIT_NAMESPACE_COL_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.WMSMOBILE.WebServicesNamespaces"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTDummy':['Enabled'],'global':['DBTWMMobileWebServicesDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTWMMobileWebServicesDetails':['Namespace','Enabled','SkipIfOffline','URL']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WEBSERVICESLISTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WEBSERVICESLISTComponent, resolver);
    }
} 