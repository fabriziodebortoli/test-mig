import { BOServiceParams, BOService, EventDataService  } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class DocumentMenuService extends BOService {
	constructor(params: BOServiceParams, eventData: EventDataService) {
		super(params, eventData);
    }
}
