import { RouterModule } from '@angular/router';
import { SettingsContainerComponent, SettingsContainerFactoryComponent } from './settings-container/settings-container.component';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

 import { TbSharedModule } from './../shared/shared.module';
 import { TbMenuModule } from './../menu/menu.module';




@NgModule({
    imports: [
        CommonModule,
        TbSharedModule,
        TbMenuModule,
        RouterModule.forChild([
          { path: 'settings', component: SettingsContainerFactoryComponent },
      ]),
      ],
  exports: [
    SettingsContainerComponent,
    SettingsContainerFactoryComponent
  ],
  declarations: [SettingsContainerComponent,SettingsContainerFactoryComponent],
  entryComponents: [SettingsContainerComponent]
})
export class TbSettingsModule {
}
