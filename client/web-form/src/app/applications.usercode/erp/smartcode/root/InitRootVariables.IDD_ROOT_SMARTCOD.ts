import { BOService, BOClient } from '@taskbuilder/core';

export class InitRootVariables extends BOClient {
    constructor(boService: BOService) {
        super(boService);
        this.boService.registerModelField('', 'nRootLength');
    }

}