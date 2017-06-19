import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';
export declare class ExplorerService extends DocumentService {
    constructor(logger: Logger, eventData: EventDataService);
    setSelectedApplication(application: any): void;
}
