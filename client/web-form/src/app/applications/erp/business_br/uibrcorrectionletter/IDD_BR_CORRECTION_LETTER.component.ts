import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_CORRECTION_LETTERService } from './IDD_BR_CORRECTION_LETTER.service';

@Component({
    selector: 'tb-IDD_BR_CORRECTION_LETTER',
    templateUrl: './IDD_BR_CORRECTION_LETTER.component.html',
    providers: [IDD_BR_CORRECTION_LETTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_CORRECTION_LETTERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_CORRECTION_LETTERService,
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
		boService.appendToModelStructure({'BRCorrectionLetterForCust':['ProgressiveNumber','CorrectionLetterDate','CCeNFeDescri','CorrectionLetterText','UseCondition'],'BRCorrectionLetterForSupp':['ProgressiveNumber','CorrectionLetterDate','CCeNFeDescri','CorrectionLetterText','UseCondition'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_CORRECTION_LETTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_CORRECTION_LETTERComponent, resolver);
    }
} 