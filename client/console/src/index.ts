import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SampleComponent } from './sample.component';
import { AdminToolbarComponent } from './components/admin-toolbar/admin-toolbar.component';
import { AdminSidenavContainerComponent } from './components/admin-sidenav-container/admin-sidenav-container.component';
import { AdminSidenavComponent } from './components/admin-sidenav/admin-sidenav.component';

// import { SampleDirective } from './sample.directive';
// import { SamplePipe } from './sample.pipe';
// import { SampleService } from './sample.service';

import { MaterialModule, MdSidenavContainer, MdSidenavModule, MdToolbarModule, MdButtonModule } from '@angular/material';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';

export * from './sample.component';
// export * from './sample.directive';
// export * from './sample.pipe';
// export * from './sample.service';

@NgModule({
  imports: [
    MaterialModule,
    MdSidenavModule,
    CommonModule,
    MdButtonModule,
    MdToolbarModule,
    BrowserAnimationsModule
  ],
  declarations: [
    SampleComponent,
    AdminToolbarComponent,
    AdminSidenavContainerComponent,
    AdminSidenavComponent
    // SampleDirective,
    // SamplePipe
  ],
  exports: [
    SampleComponent,
    AdminToolbarComponent,
    AdminSidenavContainerComponent,
    AdminSidenavComponent
    // SampleDirective,
    // SamplePipe
  ]
})
export class ConsoleModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ConsoleModule,
      // providers: [SampleService]
    };
  }
}