import { Injectable, OnDestroy, NgZone } from '@angular/core';
import {WebSocketService} from './websocket.service';
import * as _ from 'lodash';

export type HyperLinkInfo = {ns: string, cmpId: string};

@Injectable()
export class HyperLinkService {

    constructor(private wsService: WebSocketService){

    }

    public async follow(p: HyperLinkInfo): Promise<void> {
        await this.wsService.openHyperLink(p.ns, p.cmpId);
    }
}