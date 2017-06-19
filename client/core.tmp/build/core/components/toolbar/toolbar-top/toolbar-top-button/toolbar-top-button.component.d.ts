import { HttpService } from './../../../../services/http.service';
import { EventDataService } from './../../../../services/eventdata.service';
import { TbComponent } from './../../..';
export declare class ToolbarTopButtonComponent extends TbComponent {
    private eventData;
    private httpService;
    caption: string;
    disabled: boolean;
    iconType: string;
    icon: string;
    imgUrl: string;
    constructor(eventData: EventDataService, httpService: HttpService);
    onCommand(): void;
}
