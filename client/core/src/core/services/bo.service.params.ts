import { DocumentServiceParams } from './document.service.params';
import { InfoService } from './info.service';
import { HttpService } from './http.service';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { WebSocketService } from './websocket.service';
import { Injectable } from '@angular/core';
@Injectable()
export class BOServiceParams extends DocumentServiceParams {
    constructor(
        logger: Logger,
        infoService: InfoService,
        httpService: HttpService,
        eventData: EventDataService,
        public webSocketService: WebSocketService) {
        super(logger, infoService, httpService, eventData);
    }
}