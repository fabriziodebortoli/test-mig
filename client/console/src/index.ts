import { NgModule, ModuleWithProviders } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule, MdSidenavContainer, MdSidenavModule, MdToolbarModule, MdButtonModule, MdListModule } from '@angular/material';
import { CommonModule } from '@angular/common';
import { SampleComponent } from './sample.component';
import { AdminToolbarComponent } from './components/admin-toolbar/admin-toolbar.component';
import { AdminSidenavContainerComponent } from './components/admin-sidenav-container/admin-sidenav-container.component';
import { AdminSidenavComponent } from './components/admin-sidenav/admin-sidenav.component';
import { AdminListComponent } from "./components/admin-list/admin-list.component";
import { AdminCheckBoxComponent } from "./components/admin-checkbox/admin-checkbox.component";

// import { SampleDirective } from './sample.directive';
// import { SamplePipe } from './sample.pipe';
// import { SampleService } from './sample.service';

export * from './sample.component';
// export * from './sample.directive';
// export * from './sample.pipe';
// export * from './sample.service';

@NgModule({
  imports: [
    BrowserAnimationsModule,
    MaterialModule,
    MdSidenavModule,
    CommonModule,
    MdButtonModule,
    MdToolbarModule,
    MdListModule
  ],
  declarations: [
    SampleComponent,
    AdminToolbarComponent,
    AdminSidenavContainerComponent,
    AdminSidenavComponent,
    AdminListComponent,
    AdminCheckBoxComponent
    // SampleDirective,
    // SamplePipe
  ],
  exports: [
    SampleComponent,
    AdminToolbarComponent,
    AdminSidenavContainerComponent,
    AdminSidenavComponent,
    AdminListComponent,
    AdminCheckBoxComponent
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