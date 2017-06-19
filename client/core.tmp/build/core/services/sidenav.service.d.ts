import { Observable } from 'rxjs/Rx';
export declare class SidenavService {
    private sidenavOpenedSource;
    sidenavOpened$: Observable<boolean>;
    toggleSidenav(): void;
}
