import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DECLARATION_OF_INTENT_NUMBERSService } from './IDD_DECLARATION_OF_INTENT_NUMBERS.service';

@Component({
    selector: 'tb-IDD_DECLARATION_OF_INTENT_NUMBERS',
    templateUrl: './IDD_DECLARATION_OF_INTENT_NUMBERS.component.html',
    providers: [IDD_DECLARATION_OF_INTENT_NUMBERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DECLARATION_OF_INTENT_NUMBERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DECLARATION_OF_INTENT_NUMBERSService,
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
		boService.appendToModelStructure({'DeclarationOfIntentNumbers':['Received','LastDate','LastLogNo','Suffix','LastPrintingDate','DefinitivelyPrinted'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DECLARATION_OF_INTENT_NUMBERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DECLARATION_OF_INTENT_NUMBERSComponent, resolver);
    }
} 