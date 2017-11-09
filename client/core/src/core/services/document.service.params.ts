import { Injectable } from '@angular/core';
import { EventDataService } from './eventdata.service';
import { HttpService } from './http.service';
import { InfoService } from './info.service';
import { Logger } from './logger.service';
import { TbComponentServiceParams } from './tbcomponent.service.params';
@Injectable()
export class DocumentServiceParams extends TbComponentServiceParams {
    constructor(
        logger: Logger,
        infoService: InfoService,
        httpService: HttpService,
        public eventData: EventDataService) {
        super(logger, infoService, httpService);
    }

}