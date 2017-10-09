import { ProductInfoDialogComponent } from './product-info-dialog/product-info-dialog.component';
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
    SettingsContainerFactoryComponent,
    ProductInfoDialogComponent
  ],
   declarations: [SettingsContainerComponent,SettingsContainerFactoryComponent,ProductInfoDialogComponent],
  entryComponents: [SettingsContainerComponent,ProductInfoDialogComponent]
})
export class TbSettingsModule {
}
