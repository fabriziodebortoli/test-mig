import { BOServiceParams, BOService, EventDataService  } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class @@NAME@@Service extends BOService {
	constructor(params: BOServiceParams, eventData: EventDataService) {
		super(params, eventData);
@@CONSTRUCTORCODE@@
    }


}
