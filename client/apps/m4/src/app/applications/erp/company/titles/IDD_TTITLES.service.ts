import { WebSocketService, BOService, Logger, EventDataService, InfoService  } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class IDD_TTITLESService extends BOService {
    constructor(
        webSocketService: WebSocketService,
        logger: Logger,
        eventData: EventDataService,
        infoService: InfoService) {
        super(webSocketService, eventData, logger, infoService);

    }


}
