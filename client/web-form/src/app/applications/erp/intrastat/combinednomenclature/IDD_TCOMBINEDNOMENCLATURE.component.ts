import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TCOMBINEDNOMENCLATUREService } from './IDD_TCOMBINEDNOMENCLATURE.service';

@Component({
    selector: 'tb-IDD_TCOMBINEDNOMENCLATURE',
    templateUrl: './IDD_TCOMBINEDNOMENCLATURE.component.html',
    providers: [IDD_TCOMBINEDNOMENCLATUREService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TCOMBINEDNOMENCLATUREComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TCOMBINEDNOMENCLATUREService,
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
		boService.appendToModelStructure({'CombinedNomenclature':['CombinedNomenclature','Disabled','Description','SuppUnitCode'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TCOMBINEDNOMENCLATUREFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TCOMBINEDNOMENCLATUREComponent, resolver);
    }
} 