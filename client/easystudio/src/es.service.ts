import { Injectable, EventEmitter } from '@angular/core';

import { DocumentService, ComponentService, EventDataService, TbComponentServiceParams } from '@taskbuilder/core';

@Injectable()
export class ESService extends DocumentService {

  constructor(
    params: TbComponentServiceParams,
    eventData: EventDataService,
    public componentService: ComponentService
  ) {
    super(params, eventData);
  }

  close() {
    super.close();
    this.componentService.removeComponentById(this.mainCmpId);
  }
}
