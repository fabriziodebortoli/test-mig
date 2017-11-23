import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_F24_ONLINEService } from './IDD_F24_ONLINE.service';

@Component({
    selector: 'tb-IDD_F24_ONLINE',
    templateUrl: './IDD_F24_ONLINE.component.html',
    providers: [IDD_F24_ONLINEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_F24_ONLINEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_F24_ONLINEService,
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
        
        		this.bo.appendToModelStructure({'global':['New','ExtractFees','Activate_Tax','Activate_INPS','Activate_District','Activate_IMU','Activate_INAIL','Activate_Other','Search','ExtractFees2','ReadCBI','PaymentDate','AccrualMonth','AccrualMonthDescri','CompanyBank','Veicolatore','CompanyBankActive','TaxDetail','INPSDetail','DistrictsDetail','IMUDetail','INAILDetail','OtherDetail','UpdateFees','UpdateFeesDescri','FileName','WriteCBI','MediumName'],'PersonalData':['l_TEnhPersonalData_P49','l_TEnhPersonalData_P40','l_TEnhPersonalData_P50','l_TEnhPersonalData_P51','l_TEnhPersonalData_P47','l_TEnhPersonalData_P01','l_TEnhPersonalData_P02','l_TEnhPersonalData_P13','l_TEnhPersonalData_P16','l_TEnhPersonalData_P14','l_TEnhPersonalData_P15','l_TEnhPersonalData_P17','l_TEnhPersonalData_P03','l_TEnhPersonalData_P04','l_TEnhPersonalData_P07','l_TEnhPersonalData_P08','l_TEnhPersonalData_P06','l_TEnhPersonalData_P05','l_TEnhPersonalData_P11','l_TEnhPersonalData_P09','l_TEnhPersonalData_P10','l_TEnhPersonalData_P12','l_TEnhPersonalData_P60','l_TEnhPersonalData_P73','l_TEnhPersonalData_P71','l_TEnhPersonalData_P69','l_TEnhPersonalData_P70','l_TEnhPersonalData_P72','l_TEnhPersonalData_P62','l_TEnhPersonalData_P63','l_TEnhPersonalData_P64','l_TEnhPersonalData_P65','l_TEnhPersonalData_P66','l_TEnhPersonalData_P67','l_TEnhPersonalData_P68','l_TEnhPersonalData_P21','l_TEnhPersonalData_P23','l_TEnhPersonalData_P22','l_TEnhPersonalData_P24','l_TEnhPersonalData_P25','l_TEnhPersonalData_P26','l_TEnhPersonalData_P27','l_TEnhPersonalData_P30','l_TEnhPersonalData_P28','l_TEnhPersonalData_P29','l_TEnhPersonalData_P31','l_TEnhPersonalData_P80','l_TEnhPersonalData_P83','l_TEnhPersonalData_P86','l_TEnhPersonalData_P89','l_TEnhPersonalData_P92','l_TEnhPersonalData_P95','l_TEnhPersonalData_P80','l_TEnhPersonalData_P83','l_TEnhPersonalData_P86','l_TEnhPersonalData_P89','l_TEnhPersonalData_P92','l_TEnhPersonalData_P95','l_TEnhPersonalData_P98'],'HKLCompanyBank':['Description'],'HKLCompanyBankActive':['Description'],'TaxDetail':['l_TEnhTAX_P01','l_TEnhTAX_P02','l_TEnhTAX_P03','l_TEnhTAX_P06','l_TEnhTAX_P07','l_TEnhTAX_P04','l_TEnhTAX_P05'],'INPSDetail':['l_TEnhINPS_P01','l_TEnhINPS_P02','l_TEnhINPS_P03','l_TEnhINPS_P04','l_TEnhINPS_P05','l_TEnhINPS_P06','l_TEnhINPS_P07'],'DistrictsDetail':['l_TEnhDISTRICTS_P01','l_TEnhDISTRICTS_P02','l_TEnhDISTRICTS_P03','l_TEnhDISTRICTS_P04','l_TEnhDISTRICTS_P05','l_TEnhDISTRICTS_P06'],'IMUDetail':['l_TEnhICI_P01','l_TEnhICI_P02','l_TEnhICI_P03','l_TEnhICI_P04','l_TEnhICI_P05','l_TEnhICI_P06','l_TEnhICI_P07','l_TEnhICI_P08','l_TEnhICI_P09','l_TEnhICI_P10','l_TEnhICI_P11','l_TEnhICI_P12','l_TEnhICI_P13'],'INAILDetail':['l_TEnhINAIL_P01','l_TEnhINAIL_P02','l_TEnhINAIL_P03','l_TEnhINAIL_P04','l_TEnhINAIL_P05','l_TEnhINAIL_P06','l_TEnhINAIL_P07'],'OtherDetail':['l_TEnhOTHER_P01','l_TEnhOTHER_P02','l_TEnhOTHER_P03','l_TEnhOTHER_P04','l_TEnhOTHER_P05','l_TEnhOTHER_P06','l_TEnhOTHER_P07','l_TEnhOTHER_P08']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_F24_ONLINEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_F24_ONLINEComponent, resolver);
    }
} 