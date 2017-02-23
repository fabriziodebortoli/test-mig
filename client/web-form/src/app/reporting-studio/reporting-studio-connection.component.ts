import { Subject } from 'rxjs/Subject';
import { Component, OnDestroy } from '@angular/core';

@Component({
    selector: 'tb-reporting-studio-connection',
    template: ``
})

export class ReportingStudioConnectionComponent implements OnDestroy {
    private rsServer: string = 'ws://localhost:5000/rsweb';
    websocket: WebSocket;

    public message: Subject<string> = new Subject<string>();

    constructor() {
        this.websocket = new WebSocket(this.rsServer);
        this.websocket.onopen = evt => { this.onOpen(evt) };
        this.websocket.onclose = evt => { this.onClose(evt) };
        this.websocket.onmessage = evt => { this.onMessage(evt) };
        this.websocket.onerror = evt => { this.onError(evt) };
    }

    onOpen(evt: any) {
        this.writeToScreen('CONNECTED');
    }

    onClose(evt) {
        this.writeToScreen('DISCONNECTED');
    }

    onMessage(evt) {
        this.message.next(evt.data);
        this.writeToScreen(evt.data);
    }

    onError(evt) {
        this.writeToScreen(evt.data);
    }

    doSend(message) {
        this.waitForConnection(() => {
            this.writeToScreen('SENT: ' + message);
            this.websocket.send(message);
        }, 100);
    }

    waitForConnection(callback, interval) {
        if (this.websocket.readyState === 1) {
            callback();
        } else {
            let that = this;
            setTimeout(function () {
                that.waitForConnection(callback, interval);
            }, interval);
        }
    };

    writeToScreen(message) {
        console.log(message);
    }

    closeConnection() {
        this.websocket.close();
    }



    ngOnDestroy() {
        this.closeConnection();
    }
}