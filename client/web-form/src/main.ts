import { enableProdMode, ApplicationRef } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { enableDebugTools } from '@angular/platform-browser';

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule).then(m => {
  if (!environment.production) {
    const applicationRef = m.injector.get(ApplicationRef);
    enableDebugTools(applicationRef.components[0]);
  }
});
