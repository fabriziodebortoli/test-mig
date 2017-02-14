import { UnsupportedFactoryComponent } from './unsupported.component';
import { LoginComponent } from './menu/components/login/login.component';
import { PageNotFoundComponent } from 'tb-shared';
import { HomeComponent } from './home/home.component';
import { ModuleWithProviders } from '@angular/core';
import { DataServiceComponent } from './applications/test/data-service/data-service.component';
import { Routes, RouterModule } from '@angular/router';
import { CoreGuard } from './core/core.guard';

export const appRoutes: Routes = [

    { path: '', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'home', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'Framework/TbGes/IDD_Unsupported', component: UnsupportedFactoryComponent, outlet: 'dynamic' },
    { path: 'menu', loadChildren: './menu/menu.module#MenuModule' },
    { path: 'ds', component: DataServiceComponent },
    { path: 'rs', loadChildren: './reporting-studio/reporting-studio.module#ReportingStudioModule' },
    { path: '**', component: PageNotFoundComponent },
];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes);
