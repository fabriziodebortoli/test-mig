import { HttpService } from './http.service';
import { Injectable } from '@angular/core';

import { InfoService } from './info.service';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { Observable } from '../../rxjs.imports';


@Injectable()
export class TbComponentServiceParams {
    constructor(
        public logger: Logger,
        public infoService: InfoService,
        public httpService: HttpService) {
    }
}