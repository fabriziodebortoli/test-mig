import { NgModule, ModuleWithProviders } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';
import { TbSharedModule } from '@taskbuilder/core';

import { SFMService } from './sfm.service';
export { SFMService } from './sfm.service';

import { CoreService } from './core/sfm-core.service';
export { CoreService } from './core/sfm-core.service';

import { ProcessingsService } from './core/sfm-processing.service';
export { ProcessingsService } from './core/sfm-processing.service';

import { MessagesService } from './core/sfm-message.service';
export { MessagesService } from './core/sfm-message.service';

import { SFMPageComponent, SFMPageFactoryComponent } from './sfm-page/sfm-page.component';
export { SFMPageComponent, SFMPageFactoryComponent } from './sfm-page/sfm-page.component';

import { SFMStandaloneComponent } from './sfm-standalone/sfm-standalone.component';
export { SFMStandaloneComponent } from './sfm-standalone/sfm-standalone.component';

import { elementCardComponent } from './components/elementCards/elementCard-component';
export { elementCardComponent } from './components/elementCards/elementCard-component';

import { gaugeNumberCardComponent } from './components/gaugeCards/gaugeNumberCard-component';
export { gaugeNumberCardComponent } from './components/gaugeCards/gaugeNumberCard-component';

import { gaugePercentageCardComponent } from './components/gaugeCards/gaugePercentageCard-component';
export { gaugePercentageCardComponent } from './components/gaugeCards/gaugePercentageCard-component';

// import { moComponent } from './components/workerProcessings/mos/mo-component';
// export { moComponent } from './components/workerProcessings/mos/mo-component';

import { moStepsComponent } from './components/workerProcessings/moSteps/moSteps-component';
export { moStepsComponent } from './components/workerProcessings/moSteps/moSteps-component';

import { moStepComponent } from './components/workerProcessings/moSteps/moStep-component';
export { moStepComponent } from './components/workerProcessings/moSteps/moStep-component';

import { workCentersComponent } from './components/workerProcessings/workCenters/workCenters-component';
export { workCentersComponent } from './components/workerProcessings/workCenters/workCenters-component';

import { workCenterComponent } from './components/workerProcessings/workCenters/workCenter-component';
export { workCenterComponent } from './components/workerProcessings/workCenters/workCenter-component';

import { operationsComponent } from './components/workerProcessings/operations/operations-component';
export { operationsComponent } from './components/workerProcessings/operations/operations-component';

import { operationComponent } from './components/workerProcessings/operations/operation-component';
export { operationComponent } from './components/workerProcessings/operations/operation-component';

import { workerMessagesComponent } from './components/workerMessages/workerMessages-component';
export { workerMessagesComponent } from './components/workerMessages/workerMessages-component';

import { workerMessageComponent } from './components/workerMessages/workerMessage-component';
export { workerMessageComponent } from './components/workerMessages/workerMessage-component';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbIconsModule,

    RouterModule.forChild([
      {
        path: '', component: SFMPageComponent,
        children: [
          { path: 'operations', component: operationsComponent },
          { path: 'workCenters', component: workCentersComponent },
          { path: 'moSteps', component: moStepsComponent }
        ]
      }
    ])
  ],
  declarations: [SFMPageComponent, SFMPageFactoryComponent, SFMStandaloneComponent,
    elementCardComponent, gaugeNumberCardComponent, gaugePercentageCardComponent,
    moStepsComponent, moStepComponent, 
    workCentersComponent, workCenterComponent,
    operationsComponent, operationComponent, 
    workerMessagesComponent, workerMessageComponent],
  exports: [SFMPageComponent, SFMPageFactoryComponent, SFMStandaloneComponent,
    elementCardComponent, gaugeNumberCardComponent, gaugePercentageCardComponent,
    moStepsComponent, moStepComponent, 
    workCentersComponent, workCenterComponent,
    operationsComponent, operationComponent, 
    workerMessagesComponent, workerMessageComponent],
  entryComponents: [SFMPageComponent, SFMPageFactoryComponent, SFMStandaloneComponent],
  providers: [CoreService, ProcessingsService, MessagesService]
})
export class SFMModule { }
