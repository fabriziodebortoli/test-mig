import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ISO_COUNTRIESService } from './IDD_ISO_COUNTRIES.service';

@Component({
    selector: 'tb-IDD_ISO_COUNTRIES',
    templateUrl: './IDD_ISO_COUNTRIES.component.html',
    providers: [IDD_ISO_COUNTRIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ISO_COUNTRIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ISO_COUNTRIESService,
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
		boService.appendToModelStructure({'ISOCountryCodes':['ISOCountryCode','Disabled','Description','TelephonePrefix','CountryCode','BlackList','EUCountry','SimplifiedIntrastat','NumberFormat','DateFormat','TimeFormat','Language','Currency'],'HKLLanguages':['Description'],'HKLCurrencies':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ISO_COUNTRIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ISO_COUNTRIESComponent, resolver);
    }
} 