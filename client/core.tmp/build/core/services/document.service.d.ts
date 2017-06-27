import { ViewModeType } from '../../shared/models/view-mode-type.model';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
export declare class DocumentService {
    protected logger: Logger;
    eventData: EventDataService;
    mainCmpId: string;
    constructor(logger: Logger, eventData: EventDataService);
    init(cmpId: string): void;
    dispose(): void;
    getTitle(): string;
    getViewModeType(): ViewModeType;
    close(): void;
}
