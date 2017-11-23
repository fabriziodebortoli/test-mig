import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FIXEDASSETSDEPRTPLService } from './IDD_FIXEDASSETSDEPRTPL.service';

@Component({
    selector: 'tb-IDD_FIXEDASSETSDEPRTPL',
    templateUrl: './IDD_FIXEDASSETSDEPRTPL.component.html',
    providers: [IDD_FIXEDASSETSDEPRTPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_FIXEDASSETSDEPRTPLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FIXEDASSETSDEPRTPLService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'DeprTemplate':['Template','Description','DepreciationMethod','LifePeriod','Step'],'global':['Coeff','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Coeff':['FromPeriod','ToPeriod','RegrCoeff','Perc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FIXEDASSETSDEPRTPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FIXEDASSETSDEPRTPLComponent, resolver);
    }
} 