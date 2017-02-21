import { Injectable, EventEmitter } from '@angular/core';
import { CommandType } from "./reporting-studio.model";

@Injectable()
export class ReportingStudioService {
    reportOpened: EventEmitter<string> = new EventEmitter();
}
