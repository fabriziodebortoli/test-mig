import { NgModule, ModuleWithProviders } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MdIconModule, MdSidenavContainer, MdSidenavModule, MdToolbarModule, MdButtonModule, MdListModule } from '@angular/material';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SampleComponent } from './sample.component';
import { AdminToolbarComponent } from './components/admin-toolbar/admin-toolbar.component';
import { AdminSidenavContainerComponent } from './components/admin-sidenav-container/admin-sidenav-container.component';
import { AdminSidenavComponent } from './components/admin-sidenav/admin-sidenav.component';
import { AdminListComponent } from "./components/admin-list/admin-list.component";
import { AdminCheckBoxComponent } from "./components/admin-checkbox/admin-checkbox.component";
import { AdminCheckBoxListComponent } from "./components/admin-checkbox-list/admin-checkbox-list.component";
import { AdminIconComponent } from "./components/admin-icon/admin-icon.component";
import { AdminInputTextComponent } from "./components/admin-input-text/admin-input-text.component";
import { AdminAlertComponent } from './components/admin-alert/admin-alert.component';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { AdminAutoFocusDirective } from './directives/admin-auto-focus.directive';

// import { SampleDirective } from './sample.directive';
// import { SamplePipe } from './sample.pipe';
// import { SampleService } from './sample.service';

export * from './sample.component';
// export * from './sample.directive';
// export * from './sample.pipe';
// export * from './sample.service';

@NgModule({
  imports: [
    FormsModule,
    BrowserAnimationsModule,
    ButtonsModule,
    InputsModule,
    MdSidenavModule,
    MdIconModule,
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
    AdminCheckBoxComponent,
    AdminCheckBoxListComponent,
    AdminIconComponent,
    AdminInputTextComponent,
    AdminAlertComponent,
    AdminAutoFocusDirective
    // SampleDirective,
    // SamplePipe
  ],
  exports: [
    SampleComponent,
    AdminToolbarComponent,
    AdminSidenavContainerComponent,
    AdminSidenavComponent,
    AdminListComponent,
    AdminCheckBoxComponent,
    AdminCheckBoxListComponent,
    AdminIconComponent,
    AdminInputTextComponent,
    AdminAlertComponent
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