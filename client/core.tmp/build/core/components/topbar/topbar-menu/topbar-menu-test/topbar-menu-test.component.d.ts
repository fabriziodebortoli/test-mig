import { ContextMenuItem } from '../../../../../shared';
import { ComponentService } from '../../../../services/component.service';
import { EventDataService } from '../../../../services/eventdata.service';
export declare class TopbarMenuTestComponent {
    private componentService;
    private eventDataService;
    menuElements: ContextMenuItem[];
    constructor(componentService: ComponentService, eventDataService: EventDataService);
    openDataService(): void;
    openRS(): void;
    openTBExplorer(): void;
    openTestGrid(): void;
    openTestIcons(): void;
}
