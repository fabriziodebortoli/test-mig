import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { CoreGuard } from './core/core.guard';

import { PageNotFoundComponent } from './shared/page-not-found.component';
import { UnsupportedFactoryComponent } from './unsupported.component';
import { LoginComponent } from './menu/components/login/login.component';
import { HomeComponent } from './home/home.component';

export const routing: ModuleWithProviders = RouterModule.forRoot([
    { path: '', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'home', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'Framework/TbGes/IDD_Unsupported', component: UnsupportedFactoryComponent, outlet: 'dynamic' },
    { path: 'menu', loadChildren: './menu/menu.module#MenuModule' },
    { path: 'test', loadChildren: './test/test.module#TestModule', outlet: 'dynamic' },
    { path: 'rs', loadChildren: './reporting-studio/reporting-studio.module#ReportingStudioModule', outlet: 'dynamic' },
    { path: '**', component: PageNotFoundComponent }
]);
