import { NgModule, ModuleWithProviders } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatIconModule, MatSidenavContainer, MatSidenavModule, MatToolbarModule, MatListModule } from '@angular/material';
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
import { AdminButtonComponent } from './components/admin-button/admin-button.component';
import { AdminDropDownComponent } from './components/admin-dropdown/admin-dropdown.component';
import { AdminTextAreaComponent } from './components/admin-textarea/admin-textarea.component';
import { AdminDialogComponent } from './components/admin-dialog/admin-dialog.component';
import { AdminAutoFocusDirective } from './directives/admin-auto-focus.directive';
import { AdminTabsComponent } from './components/admin-tabs/admin-tabs.component';

import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DropDownsModule} from '@progress/kendo-angular-dropdowns';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';

@NgModule({
  imports: [
    FormsModule,
    BrowserAnimationsModule,
    MatSidenavModule,
    MatIconModule,
    CommonModule,
    MatToolbarModule,
    MatListModule,
    ButtonsModule,
    InputsModule,
    DropDownsModule,
    DialogModule,
    LayoutModule
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
    AdminButtonComponent,
    AdminAutoFocusDirective,
    AdminDropDownComponent,
    AdminTextAreaComponent,
    AdminDialogComponent,
    AdminTabsComponent
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
    AdminButtonComponent,
    AdminDropDownComponent,
    AdminTextAreaComponent,
    AdminDialogComponent,
    AdminTabsComponent
  ],
  entryComponents: []
})
export class ConsoleModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ConsoleModule,
      // providers: [SampleService]
    };
  }
}