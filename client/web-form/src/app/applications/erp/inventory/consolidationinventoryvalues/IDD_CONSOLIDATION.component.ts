import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONSOLIDATIONService } from './IDD_CONSOLIDATION.service';

@Component({
    selector: 'tb-IDD_CONSOLIDATION',
    templateUrl: './IDD_CONSOLIDATION.component.html',
    providers: [IDD_CONSOLIDATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CONSOLIDATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CONSOLIDATIONService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','ConsolidationDate','DataEnd','EntryID','EntryIDFrom','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONSOLIDATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONSOLIDATIONComponent, resolver);
    }
} 