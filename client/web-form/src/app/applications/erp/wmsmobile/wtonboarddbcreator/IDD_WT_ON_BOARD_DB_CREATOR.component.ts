import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WT_ON_BOARD_DB_CREATORService } from './IDD_WT_ON_BOARD_DB_CREATOR.service';

@Component({
    selector: 'tb-IDD_WT_ON_BOARD_DB_CREATOR',
    templateUrl: './IDD_WT_ON_BOARD_DB_CREATOR.component.html',
    providers: [IDD_WT_ON_BOARD_DB_CREATORService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WT_ON_BOARD_DB_CREATORComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WT_ON_BOARD_DB_CREATORService,
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
		boService.appendToModelStructure({'global':['DBPathName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WT_ON_BOARD_DB_CREATORFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WT_ON_BOARD_DB_CREATORComponent, resolver);
    }
} 