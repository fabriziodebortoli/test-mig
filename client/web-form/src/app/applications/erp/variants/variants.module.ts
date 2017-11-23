import { IDD_VARIANTComponent, IDD_VARIANTFactoryComponent } from './variants/IDD_VARIANT.component';
import { IDD_COPY_VARIANTComponent, IDD_COPY_VARIANTFactoryComponent } from './variants/IDD_COPY_VARIANT.component';
import { IDD_LOADVARIANTComponent, IDD_LOADVARIANTFactoryComponent } from './variantloading/IDD_LOADVARIANT.component';
import { IDD_CHECKVARIANTSComponent, IDD_CHECKVARIANTSFactoryComponent } from './variantcheck/IDD_CHECKVARIANTS.component';
import { IDD_BOM_VARComponent, IDD_BOM_VARFactoryComponent } from './bomwithvariant/IDD_BOM_VAR.component';
import { IDD_LOAD_BOM_VARIANTSComponent, IDD_LOAD_BOM_VARIANTSFactoryComponent } from './bomloading/IDD_LOAD_BOM_VARIANTS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_VARIANT', component: IDD_VARIANTFactoryComponent },
            { path: 'IDD_COPY_VARIANT', component: IDD_COPY_VARIANTFactoryComponent },
            { path: 'IDD_LOADVARIANT', component: IDD_LOADVARIANTFactoryComponent },
            { path: 'IDD_CHECKVARIANTS', component: IDD_CHECKVARIANTSFactoryComponent },
            { path: 'IDD_BOM_VAR', component: IDD_BOM_VARFactoryComponent },
            { path: 'IDD_LOAD_BOM_VARIANTS', component: IDD_LOAD_BOM_VARIANTSFactoryComponent },
        ])],
    declarations: [
            IDD_VARIANTComponent, IDD_VARIANTFactoryComponent,
            IDD_COPY_VARIANTComponent, IDD_COPY_VARIANTFactoryComponent,
            IDD_LOADVARIANTComponent, IDD_LOADVARIANTFactoryComponent,
            IDD_CHECKVARIANTSComponent, IDD_CHECKVARIANTSFactoryComponent,
            IDD_BOM_VARComponent, IDD_BOM_VARFactoryComponent,
            IDD_LOAD_BOM_VARIANTSComponent, IDD_LOAD_BOM_VARIANTSFactoryComponent,
    ],
    exports: [
            IDD_VARIANTFactoryComponent,
            IDD_COPY_VARIANTFactoryComponent,
            IDD_LOADVARIANTFactoryComponent,
            IDD_CHECKVARIANTSFactoryComponent,
            IDD_BOM_VARFactoryComponent,
            IDD_LOAD_BOM_VARIANTSFactoryComponent,
    ],
    entryComponents: [
            IDD_VARIANTComponent,
            IDD_COPY_VARIANTComponent,
            IDD_LOADVARIANTComponent,
            IDD_CHECKVARIANTSComponent,
            IDD_BOM_VARComponent,
            IDD_LOAD_BOM_VARIANTSComponent,
    ]
})


export class VariantsModule { };