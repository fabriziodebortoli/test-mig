import { LocalizationService } from './localization.service';
import { TbComponentServiceParams } from './tbcomponent.service.params';
import { HttpService } from './http.service';
import { Injectable } from '@angular/core';

import { InfoService } from './info.service';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { Observable } from '../../rxjs.imports';

@Injectable()
export class TbComponentService extends LocalizationService {
    public logger: Logger;
    constructor(params: TbComponentServiceParams
    )
    {
        super(params.infoService,  params.httpService);
        this.logger = params.logger;
    }
}
