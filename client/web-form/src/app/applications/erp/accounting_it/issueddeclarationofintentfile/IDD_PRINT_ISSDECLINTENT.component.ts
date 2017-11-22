import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRINT_ISSDECLINTENTService } from './IDD_PRINT_ISSDECLINTENT.service';

@Component({
    selector: 'tb-IDD_PRINT_ISSDECLINTENT',
    templateUrl: './IDD_PRINT_ISSDECLINTENT.component.html',
    providers: [IDD_PRINT_ISSDECLINTENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PRINT_ISSDECLINTENTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ISSDECLINTENT_DECL_POSITIONCODE_itemSource: any;

    constructor(document: IDD_PRINT_ISSDECLINTENTService,
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
        this.IDC_ISSDECLINTENT_DECL_POSITIONCODE_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.PositionCode"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','FromMonthDate','ToMonthDate','FromNo','ToNo','Reprint','Corrective','ProtocolTel','ProtocolDoc','MonthlyPlafond','Export','YearDeclPresented','IntraSales','ExtraOp','SanMarinoSales','AssOp','GoodDescri','EMail','DeclDiff','PositionCode_XML','FiscalCodeDecl','Intermediary','CommitDate','FileNameComplete','bOneFileForIssuedIntent','ProcessingState']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRINT_ISSDECLINTENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRINT_ISSDECLINTENTComponent, resolver);
    }
} 