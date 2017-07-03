import { StandaloneReportComponent } from './home/standalone.report/standalone.report.component';
import { ReportingStudioFactoryComponent } from './reporting-studio/reporting-studio.component';
import { StandaloneDocumentComponent } from './home/standalone.document/standalone.document.component';
import { ProxyRouteComponent } from './proxy-route/proxy-route.component';
import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { CoreGuard } from '@taskbuilder/core';

import { PageNotFoundComponent } from '@taskbuilder/core';
import { UnsupportedFactoryComponent } from './unsupported.component';
import { LoginComponent } from '@taskbuilder/core';
import { HomeComponent } from './home/home.component';

import { appRoutes } from './applications/app.routing';

export const routing: ModuleWithProviders = RouterModule.forRoot([
    { path: '', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'home', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'document/:ns', component: StandaloneDocumentComponent, canActivate: [CoreGuard] },
    { path: 'rs/:ns', component: StandaloneReportComponent },
    {
        path: 'proxy',
        outlet: 'dynamic',
        component: ProxyRouteComponent,
        children: [
            { path: 'rs', loadChildren: 'app/reporting-studio/reporting-studio.module#ReportingStudioModule' },
            { path: 'test', loadChildren: 'app/test/test.module#TestModule' },
            { path: 'framework/tbges/IDD_Unsupported', component: UnsupportedFactoryComponent },
            ...appRoutes
        ],
    },
    { path: '**', component: PageNotFoundComponent },
]);
