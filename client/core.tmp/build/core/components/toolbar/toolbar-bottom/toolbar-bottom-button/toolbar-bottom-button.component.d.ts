import { OnInit } from '@angular/core';
import { EventDataService } from './../../../../services/eventdata.service';
export declare class ToolbarBottomButtonComponent implements OnInit {
    private eventData;
    caption: string;
    cmpId: string;
    disabled: boolean;
    constructor(eventData: EventDataService);
    ngOnInit(): void;
    onCommand(): void;
}
