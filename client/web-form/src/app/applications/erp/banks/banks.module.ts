import { IDD_SEARCHBANKBRANCHComponent, IDD_SEARCHBANKBRANCHFactoryComponent } from './searchbankbranch/IDD_SEARCHBANKBRANCH.component';
import { IDD_BANKSDOWNLOADERComponent, IDD_BANKSDOWNLOADERFactoryComponent } from './banksdownload/IDD_BANKSDOWNLOADER.component';
import { IDD_BANKS_AZComponent, IDD_BANKS_AZFactoryComponent } from './uibanks/IDD_BANKS_AZ.component';
import { IDD_BANKSComponent, IDD_BANKSFactoryComponent } from './uibanks/IDD_BANKS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SEARCHBANKBRANCH', component: IDD_SEARCHBANKBRANCHFactoryComponent },
            { path: 'IDD_BANKSDOWNLOADER', component: IDD_BANKSDOWNLOADERFactoryComponent },
            { path: 'IDD_BANKS_AZ', component: IDD_BANKS_AZFactoryComponent },
            { path: 'IDD_BANKS', component: IDD_BANKSFactoryComponent },
        ])],
    declarations: [
            IDD_SEARCHBANKBRANCHComponent, IDD_SEARCHBANKBRANCHFactoryComponent,
            IDD_BANKSDOWNLOADERComponent, IDD_BANKSDOWNLOADERFactoryComponent,
            IDD_BANKS_AZComponent, IDD_BANKS_AZFactoryComponent,
            IDD_BANKSComponent, IDD_BANKSFactoryComponent,
    ],
    exports: [
            IDD_SEARCHBANKBRANCHFactoryComponent,
            IDD_BANKSDOWNLOADERFactoryComponent,
            IDD_BANKS_AZFactoryComponent,
            IDD_BANKSFactoryComponent,
    ],
    entryComponents: [
            IDD_SEARCHBANKBRANCHComponent,
            IDD_BANKSDOWNLOADERComponent,
            IDD_BANKS_AZComponent,
            IDD_BANKSComponent,
    ]
})


export class BanksModule { };