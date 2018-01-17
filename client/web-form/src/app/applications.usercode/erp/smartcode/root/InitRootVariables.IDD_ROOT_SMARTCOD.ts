import { Observable } from 'rxjs/Rx';

import { BOService, BOClient, MessageDlgArgs } from '@taskbuilder/core';

export class InitRootVariables extends BOClient {
    constructor(
        boService: BOService) {
        super(boService);
    }

    onCommand(id: string): Observable<boolean> {
        return Observable.create(observer => {
            if (id === 'ID_EXTDOC_SAVE') {
            }
            observer.next(true);
            observer.complete();

        });
    }
}