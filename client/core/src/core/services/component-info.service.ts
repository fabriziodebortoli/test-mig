import { ComponentFactory, Inject, forwardRef } from '@angular/core';

import { ComponentInfo } from './../../shared/models/component-info.model';
import { InfoService } from './info.service';

export class ComponentInfoService {
    public componentInfo: ComponentInfo;
    constructor(
        @Inject(forwardRef(() => InfoService)) public globalInfoService: InfoService)
    { }
    public getComponentId() {
        return this.componentInfo ? this.componentInfo.id : '';
    }
}