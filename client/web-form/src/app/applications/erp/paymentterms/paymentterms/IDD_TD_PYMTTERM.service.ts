import { CheckDescriptionLength } from './../../../../applications.usercode/ERP/PaymentTerms/PaymentTerms/CheckDescriptionLength.IDD_TD_PYMTTERM';
import { CheckNotesLength } from './../../../../applications.usercode/ERP/PaymentTerms/PaymentTerms/CheckNotesLength.IDD_TD_PYMTTERM';
import { BOServiceParams, BOService, EventDataService  } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class IDD_TD_PYMTTERMService extends BOService {
	constructor(params: BOServiceParams, eventData: EventDataService) {
		super(params, eventData);
			this.boClients.push(new CheckDescriptionLength(this));
			this.boClients.push(new CheckNotesLength(this));

    }


}
