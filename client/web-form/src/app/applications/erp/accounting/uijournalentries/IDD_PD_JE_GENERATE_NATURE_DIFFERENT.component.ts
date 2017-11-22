import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_JE_GENERATE_NATURE_DIFFERENTService } from './IDD_PD_JE_GENERATE_NATURE_DIFFERENT.service';

@Component({
    selector: 'tb-IDD_PD_JE_GENERATE_NATURE_DIFFERENT',
    templateUrl: './IDD_PD_JE_GENERATE_NATURE_DIFFERENT.component.html',
    providers: [IDD_PD_JE_GENERATE_NATURE_DIFFERENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PD_JE_GENERATE_NATURE_DIFFERENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_JE_GENERATE_NATURE_DIFFERENTService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ForecastEdit','DefinitiveEdit','ReverseEdit','Simulation','GenPybleRcvbleAutoEdit','GenPybleRcvbleARequestEdit','NotGenPybleRcvbleEdit','GenerateCostAccountingAutomatic','NotGenerateCostAccounting','PostDate','AccrualDate','DocDate','ValueDate','DocNo','PostDate','AccrualDate','DocDate','ValueDate','DocNo','TaxJournal','TaxAccrualDate','PlafondAccrualDate','EUTaxJournal']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_JE_GENERATE_NATURE_DIFFERENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_JE_GENERATE_NATURE_DIFFERENTComponent, resolver);
    }
} 