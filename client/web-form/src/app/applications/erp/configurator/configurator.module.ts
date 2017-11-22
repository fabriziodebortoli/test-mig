import { IDD_QUESTIONS_CRITERIA_COPYComponent, IDD_QUESTIONS_CRITERIA_COPYFactoryComponent } from './questions/IDD_QUESTIONS_CRITERIA_COPY.component';
import { IDD_QUESTIONSComponent, IDD_QUESTIONSFactoryComponent } from './questions/IDD_QUESTIONS.component';
import { IDD_LOAD_QUESTIONComponent, IDD_LOAD_QUESTIONFactoryComponent } from './questionload/IDD_LOAD_QUESTION.component';
import { IDD_PARAMETERS_CONFIGURATORComponent, IDD_PARAMETERS_CONFIGURATORFactoryComponent } from './configuratorparameters/IDD_PARAMETERS_CONFIGURATOR.component';
import { IDD_CONFIGURATIONSComponent, IDD_CONFIGURATIONSFactoryComponent } from './configuration/IDD_CONFIGURATIONS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_QUESTIONS_CRITERIA_COPY', component: IDD_QUESTIONS_CRITERIA_COPYFactoryComponent },
            { path: 'IDD_QUESTIONS', component: IDD_QUESTIONSFactoryComponent },
            { path: 'IDD_LOAD_QUESTION', component: IDD_LOAD_QUESTIONFactoryComponent },
            { path: 'IDD_PARAMETERS_CONFIGURATOR', component: IDD_PARAMETERS_CONFIGURATORFactoryComponent },
            { path: 'IDD_CONFIGURATIONS', component: IDD_CONFIGURATIONSFactoryComponent },
        ])],
    declarations: [
            IDD_QUESTIONS_CRITERIA_COPYComponent, IDD_QUESTIONS_CRITERIA_COPYFactoryComponent,
            IDD_QUESTIONSComponent, IDD_QUESTIONSFactoryComponent,
            IDD_LOAD_QUESTIONComponent, IDD_LOAD_QUESTIONFactoryComponent,
            IDD_PARAMETERS_CONFIGURATORComponent, IDD_PARAMETERS_CONFIGURATORFactoryComponent,
            IDD_CONFIGURATIONSComponent, IDD_CONFIGURATIONSFactoryComponent,
    ],
    exports: [
            IDD_QUESTIONS_CRITERIA_COPYFactoryComponent,
            IDD_QUESTIONSFactoryComponent,
            IDD_LOAD_QUESTIONFactoryComponent,
            IDD_PARAMETERS_CONFIGURATORFactoryComponent,
            IDD_CONFIGURATIONSFactoryComponent,
    ],
    entryComponents: [
            IDD_QUESTIONS_CRITERIA_COPYComponent,
            IDD_QUESTIONSComponent,
            IDD_LOAD_QUESTIONComponent,
            IDD_PARAMETERS_CONFIGURATORComponent,
            IDD_CONFIGURATIONSComponent,
    ]
})


export class ConfiguratorModule { };