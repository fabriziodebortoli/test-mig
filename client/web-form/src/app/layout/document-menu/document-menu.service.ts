import { BOServiceParams, BOService, EventDataService, ComponentService } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class DocumentMenuService extends BOService {
	constructor(
		params: BOServiceParams, 
		eventData: EventDataService,
		public componentService: ComponentService) {
		super(params, eventData);
	}
	
	close() {
        super.close();
        this.componentService.removeComponentById(this.mainCmpId);
    }
}
