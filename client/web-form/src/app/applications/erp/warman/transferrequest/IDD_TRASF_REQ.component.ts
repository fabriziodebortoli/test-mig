import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRASF_REQService } from './IDD_TRASF_REQ.service';

@Component({
    selector: 'tb-IDD_TRASF_REQ',
    templateUrl: './IDD_TRASF_REQ.component.html',
    providers: [IDD_TRASF_REQService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TRASF_REQComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TRASF_REQ_DOC_TYPE_itemSource: any;
public IDC_TRASF_REQ_STATUS_itemSource: any;

    constructor(document: IDD_TRASF_REQService,
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
        this.IDC_TRASF_REQ_DOC_TYPE_itemSource = {
  "name": "DocumentTypeEnumCombo",
  "namespace": "ERP.WarMan.Documents.TRDocTypeItemSource"
}; 
this.IDC_TRASF_REQ_STATUS_itemSource = {
  "name": "TRStatusEnumCombo",
  "namespace": "ERP.WarMan.Documents.MOStatusItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'TransferRequest':['TRID','MONo','TRNumber','DocumentType','DocumentNumber','CreationDate','RequiredDate','TRStatus','Item','Lot','UoM','RequiredQty','ReleasedQty','ProcessedQty','ConfirmedTOQty','PickedQty','DeliveredQty','Storage','Notes'],'HKLItem':['Description'],'HKLStorages':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRASF_REQFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRASF_REQComponent, resolver);
    }
} 