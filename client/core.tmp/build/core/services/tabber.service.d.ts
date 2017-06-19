import { Observable } from 'rxjs/Rx';
export declare class TabberService {
    private tabSelectedSource;
    tabSelected$: Observable<number>;
    selectTab(index: number): void;
}
