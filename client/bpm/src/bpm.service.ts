import { Injectable, EventEmitter } from '@angular/core';

import { Logger, InfoService, DocumentService, ComponentService, EventDataService } from '@taskbuilder/core';

@Injectable()
export class BPMService extends DocumentService {

  constructor(
    public logger: Logger,
    public eventData: EventDataService,
    public componentService: ComponentService,
    public infoService: InfoService
  ) {
    super(logger, eventData, infoService);
  }

  close() {
    super.close();
    this.componentService.removeComponentById(this.mainCmpId);
  }
}
