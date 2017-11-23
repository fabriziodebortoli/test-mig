import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TECHNICAL_DATA_DEFINITIONService } from './IDD_TECHNICAL_DATA_DEFINITION.service';

@Component({
    selector: 'tb-IDD_TECHNICAL_DATA_DEFINITION',
    templateUrl: './IDD_TECHNICAL_DATA_DEFINITION.component.html',
    providers: [IDD_TECHNICAL_DATA_DEFINITIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TECHNICAL_DATA_DEFINITIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TECHNICAL_DATA_DEFINITIONService,
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
		boService.appendToModelStructure({'CommodityCategories':['Category','Description'],'global':['TechnicalDataDefinition','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TECHNICAL_DATA_DEFINITIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TECHNICAL_DATA_DEFINITIONComponent, resolver);
    }
} 