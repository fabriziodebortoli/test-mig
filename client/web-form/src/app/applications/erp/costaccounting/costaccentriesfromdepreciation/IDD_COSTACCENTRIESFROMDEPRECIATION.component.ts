import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCENTRIESFROMDEPRECIATIONService } from './IDD_COSTACCENTRIESFROMDEPRECIATION.service';

@Component({
    selector: 'tb-IDD_COSTACCENTRIESFROMDEPRECIATION',
    templateUrl: './IDD_COSTACCENTRIESFROMDEPRECIATION.component.html',
    providers: [IDD_COSTACCENTRIESFROMDEPRECIATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTACCENTRIESFROMDEPRECIATIONComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BATCH_CA_DEPR_NATURE_itemSource: any;

    constructor(document: IDD_COSTACCENTRIESFROMDEPRECIATIONService,
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
        this.IDC_BATCH_CA_DEPR_NATURE_itemSource = {
  "name": "EntryTypeNoBudgetEnumCombo",
  "namespace": "ERP.CostAccounting.Components.EntryTypeNoBudgetEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Twelfth','SimulationDate','strCalcType','TwelfthCalc','TwelfthCaption','DaysCalc','DaysCaption','Nature','PostDateDepr','AccrualDateDepr','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCENTRIESFROMDEPRECIATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCENTRIESFROMDEPRECIATIONComponent, resolver);
    }
} 