import { TabberComponent, TabComponent, TileManagerComponent, TileGroupComponent, TileComponent } from './containers';
import { EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent } from './controls/';
import { DynamicCmpComponent } from './dynamic-cmp.component';
import { MaterialModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { PageNotFoundComponent } from './page-not-found.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToolbarComponent} from './toolbar/toolbar.component';
import { ToolbarButtonComponent } from './toolbar/toolbarbutton.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [
    PageNotFoundComponent, ToolbarComponent, ToolbarButtonComponent, 
    TabComponent, TabberComponent, DynamicCmpComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent
    ],
  exports: [
    CommonModule, PageNotFoundComponent, TabComponent, TabberComponent,
    EditComponent, ComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent,
    TileManagerComponent, TileGroupComponent, TileComponent,
    ToolbarButtonComponent, ToolbarComponent, DynamicCmpComponent],
  providers:[]
})
export class SharedModule { }
