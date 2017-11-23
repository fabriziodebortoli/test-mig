import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEPRECIATIONService } from './IDD_DEPRECIATION.service';

@Component({
    selector: 'tb-IDD_DEPRECIATION',
    templateUrl: './IDD_DEPRECIATION.component.html',
    providers: [IDD_DEPRECIATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DEPRECIATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEPRECIATIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Twelfth','FiscalYearDescri','AllCtgs','CtgSel','FromCtg','ToCtg','Posted','PostDate','Block','FixedCalc','TwelfthCalc','TwelfthCaption','DaysCalc','DaysCaption','Accntng','AccPostingDate','AccrualDate','NrDoc','Values','Process']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEPRECIATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEPRECIATIONComponent, resolver);
    }
} 