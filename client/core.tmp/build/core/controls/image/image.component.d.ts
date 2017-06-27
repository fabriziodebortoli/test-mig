import { ControlComponent } from './../control.component';
import { HttpService } from './../../services/http.service';
export declare class ImageComponent extends ControlComponent {
    private httpService;
    width: number;
    height: number;
    title: string;
    constructor(httpService: HttpService);
    getStyles(): {};
    getImageUrl(namespace: string): string;
}
