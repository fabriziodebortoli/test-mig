import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { AccountService } from './services/account.service';
import { AppComponent } from './app.component';
import { JsonVisualizerPipe } from './json-visualizer.pipe';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule, MdToolbarModule } from '@angular/material';

@NgModule({
  declarations: [
    AppComponent,
    JsonVisualizerPipe
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpModule,
    MaterialModule.forRoot(),
    MdToolbarModule
  ],
  providers: [AccountService],
  bootstrap: [AppComponent]
})
export class AppModule { }
