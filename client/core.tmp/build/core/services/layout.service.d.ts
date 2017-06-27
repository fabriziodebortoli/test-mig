import { Observable } from 'rxjs/Rx';
export declare class LayoutService {
    private viewHeight;
    setViewHeight(viewHeight: number): void;
    getViewHeight(): Observable<number>;
}
