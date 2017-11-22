import { IDD_IMAGO_LINKSComponent, IDD_IMAGO_LINKSFactoryComponent } from './imagolinks/IDD_IMAGO_LINKS.component';
import { IDD_IMAGO_CHECKLINKComponent, IDD_IMAGO_CHECKLINKFactoryComponent } from './imagolinks/IDD_IMAGO_CHECKLINK.component';
import { IDD_IMAGOCONF_WIZARDComponent, IDD_IMAGOCONF_WIZARDFactoryComponent } from './configwizard/IDD_IMAGOCONF_WIZARD.component';
import { IDD_IMAGO_INTROComponent, IDD_IMAGO_INTROFactoryComponent } from './configuration/IDD_IMAGO_INTRO.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_IMAGO_LINKS', component: IDD_IMAGO_LINKSFactoryComponent },
            { path: 'IDD_IMAGO_CHECKLINK', component: IDD_IMAGO_CHECKLINKFactoryComponent },
            { path: 'IDD_IMAGOCONF_WIZARD', component: IDD_IMAGOCONF_WIZARDFactoryComponent },
            { path: 'IDD_IMAGO_INTRO', component: IDD_IMAGO_INTROFactoryComponent },
        ])],
    declarations: [
            IDD_IMAGO_LINKSComponent, IDD_IMAGO_LINKSFactoryComponent,
            IDD_IMAGO_CHECKLINKComponent, IDD_IMAGO_CHECKLINKFactoryComponent,
            IDD_IMAGOCONF_WIZARDComponent, IDD_IMAGOCONF_WIZARDFactoryComponent,
            IDD_IMAGO_INTROComponent, IDD_IMAGO_INTROFactoryComponent,
    ],
    exports: [
            IDD_IMAGO_LINKSFactoryComponent,
            IDD_IMAGO_CHECKLINKFactoryComponent,
            IDD_IMAGOCONF_WIZARDFactoryComponent,
            IDD_IMAGO_INTROFactoryComponent,
    ],
    entryComponents: [
            IDD_IMAGO_LINKSComponent,
            IDD_IMAGO_CHECKLINKComponent,
            IDD_IMAGOCONF_WIZARDComponent,
            IDD_IMAGO_INTROComponent,
    ]
})


export class IMagoModule { };