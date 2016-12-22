import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent } from './containers';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { MaterialModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { PageNotFoundComponent } from './page-not-found.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TopbarComponent } from './topbar/topbar.component';
// import { TopbarButtonComponent } from './topbar/topbar-button.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [
    PageNotFoundComponent, TopbarComponent,
    // TopbarButtonComponent,
    TabComponent, TabberComponent, DynamicCmpComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent
  ],
  exports: [
    CommonModule, PageNotFoundComponent, TabComponent, TabberComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent,
    // TopbarButtonComponent, 
    TopbarComponent, DynamicCmpComponent],
  providers: []
})
export class SharedModule { }
