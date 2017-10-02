import { InfoService } from './info.service';
import { Injectable } from '@angular/core';

import { ViewModeType } from '../../shared/models';

import { Logger } from './logger.service';

import { EventDataService } from './eventdata.service';

@Injectable()
export class TbComponentService {
    constructor(public logger: Logger, public infoService: InfoService) { }
}
