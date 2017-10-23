import { Injectable } from '@angular/core';
import { Subject, Observable } from '../../rxjs.imports';

@Injectable()
export class DiagnosticService {

    messages = [];
    isVisible: boolean = false;

    observer: Subject<any>;

    constructor() { }

    showDiagnostic(messages) {
        this.observer = new Subject<any>();
        this.messages = messages;
        this.isVisible = true;
        return this.observer;
    }

    resetDiagnostic() {
        this.messages = [];
        this.isVisible = false;

        this.observer.next(true);
        this.observer.complete();
    }
}
