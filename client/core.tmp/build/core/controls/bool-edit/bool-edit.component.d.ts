import { ControlComponent } from './../control.component';
import { EventDataService } from './../../services/eventdata.service';
export declare class BoolEditComponent extends ControlComponent {
    private eventData;
    yesText: string;
    noText: string;
    constructor(eventData: EventDataService);
    keyPress(event: any): void;
    onBlur(): void;
}
