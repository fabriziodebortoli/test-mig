import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BRAUTONUMBERINGService } from './IDD_BRAUTONUMBERING.service';

@Component({
    selector: 'tb-IDD_BRAUTONUMBERING',
    templateUrl: './IDD_BRAUTONUMBERING.component.html',
    providers: [IDD_BRAUTONUMBERINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BRAUTONUMBERINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BRAUTONUMBERINGService,
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
		boService.appendToModelStructure({'BRAutoNumbering':['AutoNumbering','AutonumberingType','MaxChars','LastNo'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BRAUTONUMBERINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BRAUTONUMBERINGComponent, resolver);
    }
} 