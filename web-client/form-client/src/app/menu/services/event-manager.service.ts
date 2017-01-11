import { Injectable } from '@angular/core';
import { Observable, Observer } from 'rxjs/Rx';

@Injectable()
export class EventManagerService {

    preferenceLoaded: Observable<any>;
    preferenceLoadObserver: Observer<any>;

    tileHidden: Observable<any>;
    tileHiddenObserver: Observer<any>;


    constructor() {
        this.preferenceLoaded = new Observable((observer: Observer<any>) => {
            this.preferenceLoadObserver = observer;
        });

        this.tileHidden = new Observable((observer: Observer<any>) => {
            this.tileHiddenObserver = observer;
        });
    }

    emitPreferenceLoaded() {
        this.preferenceLoadObserver.next(undefined);
    }

    emitTileHidden(tile) {
        this.tileHiddenObserver.next(tile);
    }
}