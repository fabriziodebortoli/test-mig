import { OnInit, OnDestroy } from '@angular/core';
import { ControlTypes } from '../../shared/models/control-types.enum';
import { EventDataService } from '../services/eventdata.service';
import { BOService } from '../services/bo.service';
import { DocumentComponent } from './document.component';
export declare abstract class BOComponent extends DocumentComponent implements OnInit, OnDestroy {
    bo: BOService;
    controlTypeModel: typeof ControlTypes;
    constructor(bo: BOService, eventData: EventDataService);
    ngOnInit(): void;
    ngOnDestroy(): void;
}
