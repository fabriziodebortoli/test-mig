import { IDD_CPHELPComponent, IDD_CPHELPFactoryComponent } from './smartcodehelp/IDD_CPHELP.component';
import { IDD_SMARTCODEComponent, IDD_SMARTCODEFactoryComponent } from './smartcode/IDD_SMARTCODE.component';
import { IDD_SEGMENTSComponent, IDD_SEGMENTSFactoryComponent } from './segments/IDD_SEGMENTS.component';
import { IDD_ROOT_SMARTCODComponent, IDD_ROOT_SMARTCODFactoryComponent } from './root/IDD_ROOT_SMARTCOD.component';
import { IDD_COMBINATIONComponent, IDD_COMBINATIONFactoryComponent } from './combination/IDD_COMBINATION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_CPHELP', component: IDD_CPHELPFactoryComponent },
            { path: 'IDD_SMARTCODE', component: IDD_SMARTCODEFactoryComponent },
            { path: 'IDD_SEGMENTS', component: IDD_SEGMENTSFactoryComponent },
            { path: 'IDD_ROOT_SMARTCOD', component: IDD_ROOT_SMARTCODFactoryComponent },
            { path: 'IDD_COMBINATION', component: IDD_COMBINATIONFactoryComponent },
        ])],
    declarations: [
            IDD_CPHELPComponent, IDD_CPHELPFactoryComponent,
            IDD_SMARTCODEComponent, IDD_SMARTCODEFactoryComponent,
            IDD_SEGMENTSComponent, IDD_SEGMENTSFactoryComponent,
            IDD_ROOT_SMARTCODComponent, IDD_ROOT_SMARTCODFactoryComponent,
            IDD_COMBINATIONComponent, IDD_COMBINATIONFactoryComponent,
    ],
    exports: [
            IDD_CPHELPFactoryComponent,
            IDD_SMARTCODEFactoryComponent,
            IDD_SEGMENTSFactoryComponent,
            IDD_ROOT_SMARTCODFactoryComponent,
            IDD_COMBINATIONFactoryComponent,
    ],
    entryComponents: [
            IDD_CPHELPComponent,
            IDD_SMARTCODEComponent,
            IDD_SEGMENTSComponent,
            IDD_ROOT_SMARTCODComponent,
            IDD_COMBINATIONComponent,
    ]
})


export class SmartCodeModule { };