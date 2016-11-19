import { LanguagesFactoryComponent } from './languages/languages.component';

import { Routes, RouterModule } from '@angular/router';
const ROUTES: Routes = [
    {
        path: 'Languages', component: LanguagesFactoryComponent
    }

];
export const routing = RouterModule.forChild(ROUTES);
