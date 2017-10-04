import { Injectable } from '@angular/core';

import { InfoService } from './info.service';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';

@Injectable()
export class TbComponentService {
    constructor(public logger: Logger, public infoService: InfoService) { }
}
