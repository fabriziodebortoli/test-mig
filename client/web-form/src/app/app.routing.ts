import { ReportingStudioFactoryComponent } from './reporting-studio/reporting-studio.component';
import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { CoreGuard } from '@taskbuilder/core';
import { PageNotFoundComponent, UnsupportedFactoryComponent } from '@taskbuilder/core';
import { ProxyRouteComponent, LoginComponent, HomeComponent, StandaloneReportComponent, StandaloneDocumentComponent } from '@taskbuilder/core';

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

