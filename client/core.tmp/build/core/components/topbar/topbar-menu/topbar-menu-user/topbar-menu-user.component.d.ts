import { OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ContextMenuItem } from '../../../../../shared';
import { LoginSessionService } from '../../../../services/login-session.service';
import { EventDataService } from '../../../../services/eventdata.service';
export declare class TopbarMenuUserComponent implements OnDestroy {
    private loginSessionService;
    private eventDataService;
    menuElements: ContextMenuItem[];
    commandSubscription: Subscription;
    constructor(loginSessionService: LoginSessionService, eventDataService: EventDataService);
    logout(): void;
    ngOnDestroy(): void;
}
