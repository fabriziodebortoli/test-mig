import { DynamicCmpComponent } from './dynamic-cmp.component';
import { MaterialModule } from '@angular/material';
import { FormsModule } from '@angular/forms';
import { TabberComponent } from './tabber/tabber.component';
import { TabComponent } from './tabber/tab.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { PageNotFoundComponent } from './page-not-found.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule.forRoot()
  ],
  declarations: [PageNotFoundComponent, ToolbarComponent, TabComponent, TabberComponent, DynamicCmpComponent],
  exports: [CommonModule, PageNotFoundComponent, ToolbarComponent, TabComponent, TabberComponent, DynamicCmpComponent]
})
export class SharedModule { }
