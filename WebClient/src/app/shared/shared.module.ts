import { DynamicCmpComponent } from './dynamic-cmp.component';
import { MaterialModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { TabberComponent } from './tabber/tabber.component';
import { TabComponent } from './tabber/tab.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToolbarComponent} from './toolbar/toolbar.component';
import { ToolbarButtonComponent } from './toolbar/toolbarbutton.component';
import { EditComponent } from './edit/edit.component';


@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [PageNotFoundComponent, EditComponent, ToolbarComponent, ToolbarButtonComponent, TabComponent, TabberComponent, DynamicCmpComponent],
  exports: [CommonModule, PageNotFoundComponent, EditComponent, TabComponent, TabberComponent, ToolbarButtonComponent, ToolbarComponent, DynamicCmpComponent],
  providers:[]
})
export class SharedModule { }
