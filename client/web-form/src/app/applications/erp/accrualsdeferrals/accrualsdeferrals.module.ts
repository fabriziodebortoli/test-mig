import { IDD_ACCRUALSDEFERRALSComponent, IDD_ACCRUALSDEFERRALSFactoryComponent } from './forecastaccrualsdeferrals/IDD_ACCRUALSDEFERRALS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_ACCRUALSDEFERRALS', component: IDD_ACCRUALSDEFERRALSFactoryComponent },
        ])],
    declarations: [
            IDD_ACCRUALSDEFERRALSComponent, IDD_ACCRUALSDEFERRALSFactoryComponent,
    ],
    exports: [
            IDD_ACCRUALSDEFERRALSFactoryComponent,
    ],
    entryComponents: [
            IDD_ACCRUALSDEFERRALSComponent,
    ]
})


export class AccrualsDeferralsModule { };