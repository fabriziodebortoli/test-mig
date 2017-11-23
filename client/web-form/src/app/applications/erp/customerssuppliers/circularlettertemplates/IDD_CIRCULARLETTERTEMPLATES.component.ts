import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CIRCULARLETTERTEMPLATESService } from './IDD_CIRCULARLETTERTEMPLATES.service';

@Component({
    selector: 'tb-IDD_CIRCULARLETTERTEMPLATES',
    templateUrl: './IDD_CIRCULARLETTERTEMPLATES.component.html',
    providers: [IDD_CIRCULARLETTERTEMPLATESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CIRCULARLETTERTEMPLATESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CIRCULARLETTERTEMPLATESService,
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
		boService.appendToModelStructure({'CircularLetterTemplates':['Template','Disabled','Description','Subject','FileNamespace','ReportNamespace','PrintAuthSect','PLDeliveryType','PLPrintType'],'global':['__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CIRCULARLETTERTEMPLATESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CIRCULARLETTERTEMPLATESComponent, resolver);
    }
} 