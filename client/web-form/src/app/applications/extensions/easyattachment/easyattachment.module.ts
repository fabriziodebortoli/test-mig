import { IDD_SOSDOCSENDER_WIZARDComponent, IDD_SOSDOCSENDER_WIZARDFactoryComponent } from './uisosdocsender/IDD_SOSDOCSENDER_WIZARD.component';
import { IDD_SOS_CONFIGURATIONComponent, IDD_SOS_CONFIGURATIONFactoryComponent } from './uisosconfiguration/IDD_SOS_CONFIGURATION.component';
import { IDD_SOSADJUSTATTACH_WIZARDComponent, IDD_SOSADJUSTATTACH_WIZARDFactoryComponent } from './uisosadjustattachments/IDD_SOSADJUSTATTACH_WIZARD.component';
import { IDD_PAPERYComponent, IDD_PAPERYFactoryComponent } from './uipaperydlg/IDD_PAPERY.component';
import { IDD_MASSIVEARCHIVE_WIZARDComponent, IDD_MASSIVEARCHIVE_WIZARDFactoryComponent } from './uimassivearchive/IDD_MASSIVEARCHIVE_WIZARD.component';
import { IDD_DMS_SETTINGSComponent, IDD_DMS_SETTINGSFactoryComponent } from './uidmssettings/IDD_DMS_SETTINGS.component';
import { IDD_DMSREPOSITORY_EXPLORERComponent, IDD_DMSREPOSITORY_EXPLORERFactoryComponent } from './uidmsrepository/IDD_DMSREPOSITORY_EXPLORER.component';
import { IDD_DMSREPOSITORY_BROWSERComponent, IDD_DMSREPOSITORY_BROWSERFactoryComponent } from './uidmsrepository/IDD_DMSREPOSITORY_BROWSER.component';
import { IDD_DMSCATEGORIESComponent, IDD_DMSCATEGORIESFactoryComponent } from './uidmscategories/IDD_DMSCATEGORIES.component';
import { IDD_ACQUISITION_FROM_DEVICEComponent, IDD_ACQUISITION_FROM_DEVICEFactoryComponent } from './uiacquisitionfromdevice/IDD_ACQUISITION_FROM_DEVICE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SOSDOCSENDER_WIZARD', component: IDD_SOSDOCSENDER_WIZARDFactoryComponent },
            { path: 'IDD_SOS_CONFIGURATION', component: IDD_SOS_CONFIGURATIONFactoryComponent },
            { path: 'IDD_SOSADJUSTATTACH_WIZARD', component: IDD_SOSADJUSTATTACH_WIZARDFactoryComponent },
            { path: 'IDD_PAPERY', component: IDD_PAPERYFactoryComponent },
            { path: 'IDD_MASSIVEARCHIVE_WIZARD', component: IDD_MASSIVEARCHIVE_WIZARDFactoryComponent },
            { path: 'IDD_DMS_SETTINGS', component: IDD_DMS_SETTINGSFactoryComponent },
            { path: 'IDD_DMSREPOSITORY_EXPLORER', component: IDD_DMSREPOSITORY_EXPLORERFactoryComponent },
            { path: 'IDD_DMSREPOSITORY_BROWSER', component: IDD_DMSREPOSITORY_BROWSERFactoryComponent },
            { path: 'IDD_DMSCATEGORIES', component: IDD_DMSCATEGORIESFactoryComponent },
            { path: 'IDD_ACQUISITION_FROM_DEVICE', component: IDD_ACQUISITION_FROM_DEVICEFactoryComponent },
        ])],
    declarations: [
            IDD_SOSDOCSENDER_WIZARDComponent, IDD_SOSDOCSENDER_WIZARDFactoryComponent,
            IDD_SOS_CONFIGURATIONComponent, IDD_SOS_CONFIGURATIONFactoryComponent,
            IDD_SOSADJUSTATTACH_WIZARDComponent, IDD_SOSADJUSTATTACH_WIZARDFactoryComponent,
            IDD_PAPERYComponent, IDD_PAPERYFactoryComponent,
            IDD_MASSIVEARCHIVE_WIZARDComponent, IDD_MASSIVEARCHIVE_WIZARDFactoryComponent,
            IDD_DMS_SETTINGSComponent, IDD_DMS_SETTINGSFactoryComponent,
            IDD_DMSREPOSITORY_EXPLORERComponent, IDD_DMSREPOSITORY_EXPLORERFactoryComponent,
            IDD_DMSREPOSITORY_BROWSERComponent, IDD_DMSREPOSITORY_BROWSERFactoryComponent,
            IDD_DMSCATEGORIESComponent, IDD_DMSCATEGORIESFactoryComponent,
            IDD_ACQUISITION_FROM_DEVICEComponent, IDD_ACQUISITION_FROM_DEVICEFactoryComponent,
    ],
    exports: [
            IDD_SOSDOCSENDER_WIZARDFactoryComponent,
            IDD_SOS_CONFIGURATIONFactoryComponent,
            IDD_SOSADJUSTATTACH_WIZARDFactoryComponent,
            IDD_PAPERYFactoryComponent,
            IDD_MASSIVEARCHIVE_WIZARDFactoryComponent,
            IDD_DMS_SETTINGSFactoryComponent,
            IDD_DMSREPOSITORY_EXPLORERFactoryComponent,
            IDD_DMSREPOSITORY_BROWSERFactoryComponent,
            IDD_DMSCATEGORIESFactoryComponent,
            IDD_ACQUISITION_FROM_DEVICEFactoryComponent,
    ],
    entryComponents: [
            IDD_SOSDOCSENDER_WIZARDComponent,
            IDD_SOS_CONFIGURATIONComponent,
            IDD_SOSADJUSTATTACH_WIZARDComponent,
            IDD_PAPERYComponent,
            IDD_MASSIVEARCHIVE_WIZARDComponent,
            IDD_DMS_SETTINGSComponent,
            IDD_DMSREPOSITORY_EXPLORERComponent,
            IDD_DMSREPOSITORY_BROWSERComponent,
            IDD_DMSCATEGORIESComponent,
            IDD_ACQUISITION_FROM_DEVICEComponent,
    ]
})


export class EasyAttachmentModule { };