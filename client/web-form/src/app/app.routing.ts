import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';
import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { CoreGuard } from '@taskbuilder/core';
import { PageNotFoundComponent, UnsupportedFactoryComponent } from '@taskbuilder/core';
import { ProxyRouteComponent, LoginComponent, HomeComponent, StandaloneReportComponent, StandaloneDocumentComponent } from '@taskbuilder/core';

import { RsTestComponent } from '@taskbuilder/reporting-studio';

//import { appRoutes } from './applications/app.routing';

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
            { path: 'rs', loadChildren: '@taskbuilder/reporting-studio#ReportingStudioModule' },
            // { path: 'test', loadChildren: 'app/test/test.module#TestModule' },
            { path: 'framework/tbges/IDD_Unsupported', component: UnsupportedFactoryComponent },
            //...appRoutes
        ],
    },
    { path: 'test-rs', component: RsTestComponent },
    { path: '**', component: PageNotFoundComponent },
]);

