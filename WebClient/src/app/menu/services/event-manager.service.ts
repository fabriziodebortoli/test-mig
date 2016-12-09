import { Observable, Observer } from 'rxjs/Rx';
import { HttpMenuService } from './http-menu.service';
import { UtilsService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';
import { ImageService } from './image.service';
import { DocumentInfo } from 'tb-shared';

@Injectable()
export class EventManagerService {

    preferenceLoaded: Observable<any>;
    preferenceLoadObserver: Observer<any>;

    constructor() {
        this.preferenceLoaded = new Observable((observer: Observer<any>) => {
            this.preferenceLoadObserver = observer;
        });
    }

    emitPreferenceLoaded() {
        this.preferenceLoadObserver.next(undefined);
    }
}