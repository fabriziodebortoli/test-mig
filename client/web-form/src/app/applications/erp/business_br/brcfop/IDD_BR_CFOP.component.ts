import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_CFOPService } from './IDD_BR_CFOP.service';

@Component({
    selector: 'tb-IDD_BR_CFOP',
    templateUrl: './IDD_BR_CFOP.component.html',
    providers: [IDD_BR_CFOPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_CFOPComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_CFOP_MOV_TYPE_itemSource: any;
public IDC_BR_CFOP_TRANS_TYPE_itemSource: any;

    constructor(document: IDD_BR_CFOPService,
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
        this.IDC_BR_CFOP_MOV_TYPE_itemSource = {
  "allowChanges": false,
  "name": "MovementTypeComboBox",
  "namespace": "ERP.Business_BR.Components.MovementTypeComboBox",
  "useProductLanguage": false
}; 
this.IDC_BR_CFOP_TRANS_TYPE_itemSource = {
  "allowChanges": false,
  "name": "TransactionTypeComboBox",
  "namespace": "ERP.Business_BR.Components.TransactionTypeComboBox",
  "useProductLanguage": false
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRCFOP':['CFOP','ShortDescription','Description','ValidityStartingDate','ValidityEndingDate','CFOPGroup','OperationDescription','ExcludeFromTot','MovementType','MoveTypeDescri','TransactionType','TransTypeDescri'],'HKLBRCFOPGroup':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_CFOPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_CFOPComponent, resolver);
    }
} 