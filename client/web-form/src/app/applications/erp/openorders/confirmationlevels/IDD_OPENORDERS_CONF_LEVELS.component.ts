import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OPENORDERS_CONF_LEVELSService } from './IDD_OPENORDERS_CONF_LEVELS.service';

@Component({
    selector: 'tb-IDD_OPENORDERS_CONF_LEVELS',
    templateUrl: './IDD_OPENORDERS_CONF_LEVELS.component.html',
    providers: [IDD_OPENORDERS_CONF_LEVELSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_OPENORDERS_CONF_LEVELSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OPENORDERS_CONF_LEVELSService,
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
		boService.appendToModelStructure({'ConfirmationLevels':['ConfirmationLevel','Description','BackgroundColour','TextColour','PrintSymbol'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OPENORDERS_CONF_LEVELSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OPENORDERS_CONF_LEVELSComponent, resolver);
    }
} 