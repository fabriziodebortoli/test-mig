import { Observable } from 'rxjs/Rx';

import { BOService, BOClient } from '@taskbuilder/core';
// , MessageDlgArgs

export class InitRootVariables extends BOClient {
    constructor(boService: BOService) {
        super(boService);
        this.boService.registerModelField('', 'nRootLength');
    }

}