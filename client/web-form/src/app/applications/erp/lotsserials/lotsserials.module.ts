import { IDD_NUMERATOR_SERIALNOS_FULLComponent, IDD_NUMERATOR_SERIALNOS_FULLFactoryComponent } from './serialnumbers/IDD_NUMERATOR_SERIALNOS_FULL.component';
import { IDD_LOTSTRACING_REBUILDINGComponent, IDD_LOTSTRACING_REBUILDINGFactoryComponent } from './lotstracingrebuilding/IDD_LOTSTRACING_REBUILDING.component';
import { IDD_LOT_SERIAL_PARAMETERSComponent, IDD_LOT_SERIAL_PARAMETERSFactoryComponent } from './lotsserialsparameters/IDD_LOT_SERIAL_PARAMETERS.component';
import { IDD_NUMERATOR_LOTSNUMBERSComponent, IDD_NUMERATOR_LOTSNUMBERSFactoryComponent } from './lotsnumbers/IDD_NUMERATOR_LOTSNUMBERS.component';
import { IDD_LOTGRAFComponent, IDD_LOTGRAFFactoryComponent } from './lotsnavigation/IDD_LOTGRAF.component';
import { IDD_LOTS_MNG_SETTINGSComponent, IDD_LOTS_MNG_SETTINGSFactoryComponent } from './lotsmanagementsettings/IDD_LOTS_MNG_SETTINGS.component';
import { IDD_LOTS_SUPP_ADD_ON_FLYComponent, IDD_LOTS_SUPP_ADD_ON_FLYFactoryComponent } from './lots/IDD_LOTS_SUPP_ADD_ON_FLY.component';
import { IDD_LOTSComponent, IDD_LOTSFactoryComponent } from './lots/IDD_LOTS.component';
import { IDD_EXPIRE_LOTS_MANAGEComponent, IDD_EXPIRE_LOTS_MANAGEFactoryComponent } from './expirelotsmanage/IDD_EXPIRE_LOTS_MANAGE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_NUMERATOR_SERIALNOS_FULL', component: IDD_NUMERATOR_SERIALNOS_FULLFactoryComponent },
            { path: 'IDD_LOTSTRACING_REBUILDING', component: IDD_LOTSTRACING_REBUILDINGFactoryComponent },
            { path: 'IDD_LOT_SERIAL_PARAMETERS', component: IDD_LOT_SERIAL_PARAMETERSFactoryComponent },
            { path: 'IDD_NUMERATOR_LOTSNUMBERS', component: IDD_NUMERATOR_LOTSNUMBERSFactoryComponent },
            { path: 'IDD_LOTGRAF', component: IDD_LOTGRAFFactoryComponent },
            { path: 'IDD_LOTS_MNG_SETTINGS', component: IDD_LOTS_MNG_SETTINGSFactoryComponent },
            { path: 'IDD_LOTS_SUPP_ADD_ON_FLY', component: IDD_LOTS_SUPP_ADD_ON_FLYFactoryComponent },
            { path: 'IDD_LOTS', component: IDD_LOTSFactoryComponent },
            { path: 'IDD_EXPIRE_LOTS_MANAGE', component: IDD_EXPIRE_LOTS_MANAGEFactoryComponent },
        ])],
    declarations: [
            IDD_NUMERATOR_SERIALNOS_FULLComponent, IDD_NUMERATOR_SERIALNOS_FULLFactoryComponent,
            IDD_LOTSTRACING_REBUILDINGComponent, IDD_LOTSTRACING_REBUILDINGFactoryComponent,
            IDD_LOT_SERIAL_PARAMETERSComponent, IDD_LOT_SERIAL_PARAMETERSFactoryComponent,
            IDD_NUMERATOR_LOTSNUMBERSComponent, IDD_NUMERATOR_LOTSNUMBERSFactoryComponent,
            IDD_LOTGRAFComponent, IDD_LOTGRAFFactoryComponent,
            IDD_LOTS_MNG_SETTINGSComponent, IDD_LOTS_MNG_SETTINGSFactoryComponent,
            IDD_LOTS_SUPP_ADD_ON_FLYComponent, IDD_LOTS_SUPP_ADD_ON_FLYFactoryComponent,
            IDD_LOTSComponent, IDD_LOTSFactoryComponent,
            IDD_EXPIRE_LOTS_MANAGEComponent, IDD_EXPIRE_LOTS_MANAGEFactoryComponent,
    ],
    exports: [
            IDD_NUMERATOR_SERIALNOS_FULLFactoryComponent,
            IDD_LOTSTRACING_REBUILDINGFactoryComponent,
            IDD_LOT_SERIAL_PARAMETERSFactoryComponent,
            IDD_NUMERATOR_LOTSNUMBERSFactoryComponent,
            IDD_LOTGRAFFactoryComponent,
            IDD_LOTS_MNG_SETTINGSFactoryComponent,
            IDD_LOTS_SUPP_ADD_ON_FLYFactoryComponent,
            IDD_LOTSFactoryComponent,
            IDD_EXPIRE_LOTS_MANAGEFactoryComponent,
    ],
    entryComponents: [
            IDD_NUMERATOR_SERIALNOS_FULLComponent,
            IDD_LOTSTRACING_REBUILDINGComponent,
            IDD_LOT_SERIAL_PARAMETERSComponent,
            IDD_NUMERATOR_LOTSNUMBERSComponent,
            IDD_LOTGRAFComponent,
            IDD_LOTS_MNG_SETTINGSComponent,
            IDD_LOTS_SUPP_ADD_ON_FLYComponent,
            IDD_LOTSComponent,
            IDD_EXPIRE_LOTS_MANAGEComponent,
    ]
})


export class LotsSerialsModule { };