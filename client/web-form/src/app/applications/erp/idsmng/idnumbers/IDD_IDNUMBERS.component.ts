import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IDNUMBERSService } from './IDD_IDNUMBERS.service';

@Component({
    selector: 'tb-IDD_IDNUMBERS',
    templateUrl: './IDD_IDNUMBERS.component.html',
    providers: [IDD_IDNUMBERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_IDNUMBERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ID_NATURE_ARCHIVECODETYPE_itemSource: any;

    constructor(document: IDD_IDNUMBERSService,
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
        this.IDC_ID_NATURE_ARCHIVECODETYPE_itemSource = {
  "name": "NatureEnumCombo",
  "namespace": "ERP.IdsMng.Services.NatureEnumCombo"
}; 

        		this.bo.appendToModelStructure({'IDNumbers':['CodeType','LastId'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IDNUMBERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IDNUMBERSComponent, resolver);
    }
} 