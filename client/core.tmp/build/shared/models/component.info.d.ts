import { ComponentFactory } from '@angular/core';
import { DocumentService } from '../../core/services/document.service';
export declare class ComponentInfo {
    factory: ComponentFactory<any>;
    id: string;
    document: DocumentService;
    args: any;
}
