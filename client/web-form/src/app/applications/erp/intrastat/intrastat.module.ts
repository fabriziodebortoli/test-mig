import { IDD_INTRASTAT_COMPANYUSER_SETTINGSComponent, IDD_INTRASTAT_COMPANYUSER_SETTINGSFactoryComponent } from './intrastatsettings/IDD_INTRASTAT_COMPANYUSER_SETTINGS.component';
import { IDD_INTRASTAT_PARAMETERSComponent, IDD_INTRASTAT_PARAMETERSFactoryComponent } from './intrastatparameters/IDD_INTRASTAT_PARAMETERS.component';
import { IDD_INTRA_LOG_NUMBERComponent, IDD_INTRA_LOG_NUMBERFactoryComponent } from './intralognumber/IDD_INTRA_LOG_NUMBER.component';
import { IDD_INTRA_SERVICESComponent, IDD_INTRA_SERVICESFactoryComponent } from './intragenerationservices/IDD_INTRA_SERVICES.component';
import { IDD_STINTRAROComponent, IDD_STINTRAROFactoryComponent } from './intrafilegenerationro/IDD_STINTRARO.component';
import { IDD_STINTRABGComponent, IDD_STINTRABGFactoryComponent } from './intrafilegenerationbg/IDD_STINTRABG.component';
import { IDD_CPAComponent, IDD_CPAFactoryComponent } from './cpa/IDD_CPA.component';
import { IDD_TCOMBINEDNOMENCLATUREComponent, IDD_TCOMBINEDNOMENCLATUREFactoryComponent } from './combinednomenclature/IDD_TCOMBINEDNOMENCLATURE.component';
import { IDD_INTRA_DISPATCHESComponent, IDD_INTRA_DISPATCHESFactoryComponent } from './uiintradispatches/IDD_INTRA_DISPATCHES.component';
import { IDD_INTRA_PURCHASEComponent, IDD_INTRA_PURCHASEFactoryComponent } from './uiintraarrivals/IDD_INTRA_PURCHASE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_INTRASTAT_COMPANYUSER_SETTINGS', component: IDD_INTRASTAT_COMPANYUSER_SETTINGSFactoryComponent },
            { path: 'IDD_INTRASTAT_PARAMETERS', component: IDD_INTRASTAT_PARAMETERSFactoryComponent },
            { path: 'IDD_INTRA_LOG_NUMBER', component: IDD_INTRA_LOG_NUMBERFactoryComponent },
            { path: 'IDD_INTRA_SERVICES', component: IDD_INTRA_SERVICESFactoryComponent },
            { path: 'IDD_STINTRARO', component: IDD_STINTRAROFactoryComponent },
            { path: 'IDD_STINTRABG', component: IDD_STINTRABGFactoryComponent },
            { path: 'IDD_CPA', component: IDD_CPAFactoryComponent },
            { path: 'IDD_TCOMBINEDNOMENCLATURE', component: IDD_TCOMBINEDNOMENCLATUREFactoryComponent },
            { path: 'IDD_INTRA_DISPATCHES', component: IDD_INTRA_DISPATCHESFactoryComponent },
            { path: 'IDD_INTRA_PURCHASE', component: IDD_INTRA_PURCHASEFactoryComponent },
        ])],
    declarations: [
            IDD_INTRASTAT_COMPANYUSER_SETTINGSComponent, IDD_INTRASTAT_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_INTRASTAT_PARAMETERSComponent, IDD_INTRASTAT_PARAMETERSFactoryComponent,
            IDD_INTRA_LOG_NUMBERComponent, IDD_INTRA_LOG_NUMBERFactoryComponent,
            IDD_INTRA_SERVICESComponent, IDD_INTRA_SERVICESFactoryComponent,
            IDD_STINTRAROComponent, IDD_STINTRAROFactoryComponent,
            IDD_STINTRABGComponent, IDD_STINTRABGFactoryComponent,
            IDD_CPAComponent, IDD_CPAFactoryComponent,
            IDD_TCOMBINEDNOMENCLATUREComponent, IDD_TCOMBINEDNOMENCLATUREFactoryComponent,
            IDD_INTRA_DISPATCHESComponent, IDD_INTRA_DISPATCHESFactoryComponent,
            IDD_INTRA_PURCHASEComponent, IDD_INTRA_PURCHASEFactoryComponent,
    ],
    exports: [
            IDD_INTRASTAT_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_INTRASTAT_PARAMETERSFactoryComponent,
            IDD_INTRA_LOG_NUMBERFactoryComponent,
            IDD_INTRA_SERVICESFactoryComponent,
            IDD_STINTRAROFactoryComponent,
            IDD_STINTRABGFactoryComponent,
            IDD_CPAFactoryComponent,
            IDD_TCOMBINEDNOMENCLATUREFactoryComponent,
            IDD_INTRA_DISPATCHESFactoryComponent,
            IDD_INTRA_PURCHASEFactoryComponent,
    ],
    entryComponents: [
            IDD_INTRASTAT_COMPANYUSER_SETTINGSComponent,
            IDD_INTRASTAT_PARAMETERSComponent,
            IDD_INTRA_LOG_NUMBERComponent,
            IDD_INTRA_SERVICESComponent,
            IDD_STINTRAROComponent,
            IDD_STINTRABGComponent,
            IDD_CPAComponent,
            IDD_TCOMBINEDNOMENCLATUREComponent,
            IDD_INTRA_DISPATCHESComponent,
            IDD_INTRA_PURCHASEComponent,
    ]
})


export class IntrastatModule { };