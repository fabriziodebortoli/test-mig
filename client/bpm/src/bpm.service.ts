import { Injectable, EventEmitter } from '@angular/core';

import { DocumentService, ComponentService, EventDataService, DocumentServiceParams } from '@taskbuilder/core';

@Injectable()
export class BPMService extends DocumentService {

  constructor(
    params: DocumentServiceParams,
    public componentService: ComponentService
  ) {
    super(params);
  }

  close() {
    super.close();
    this.componentService.removeComponentById(this.mainCmpId);
  }
}
