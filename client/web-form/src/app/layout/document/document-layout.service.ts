import { BOServiceParams, BOService, EventDataService  } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class DocumentLayoutService extends BOService {
	constructor(params: BOServiceParams, eventData: EventDataService) {
		super(params, eventData);
    }
}
