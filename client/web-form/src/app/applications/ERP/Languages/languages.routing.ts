import { LanguagesFactoryComponent } from './languages/IDD_LANGUAGES.component';

import { Routes, RouterModule } from '@angular/router';
const ROUTES: Routes = [
    {
        path: 'IDD_LANGUAGES_FRAME', component: LanguagesFactoryComponent,
    }
];
export const routing = RouterModule.forChild(ROUTES);
