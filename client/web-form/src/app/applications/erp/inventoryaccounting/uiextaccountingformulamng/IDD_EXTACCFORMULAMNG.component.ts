import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EXTACCFORMULAMNGService } from './IDD_EXTACCFORMULAMNG.service';

@Component({
    selector: 'tb-IDD_EXTACCFORMULAMNG',
    templateUrl: './IDD_EXTACCFORMULAMNG.component.html',
    providers: [IDD_EXTACCFORMULAMNGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EXTACCFORMULAMNGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EXTACCFORMULAMNGService,
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
		boService.appendToModelStructure({'global':['SymbolExtAccountingFormula','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SymbolExtAccountingFormula':['VExtAccFormulaSymbol_p1']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EXTACCFORMULAMNGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EXTACCFORMULAMNGComponent, resolver);
    }
} 