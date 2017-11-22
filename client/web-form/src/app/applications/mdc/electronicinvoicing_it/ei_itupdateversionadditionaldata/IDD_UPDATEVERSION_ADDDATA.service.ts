import { BOServiceParams, BOService, EventDataService  } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class IDD_UPDATEVERSION_ADDDATAService extends BOService {
	constructor(params: BOServiceParams, eventData: EventDataService) {
		super(params, eventData);

    }


}
