import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'workerMessage',
    templateUrl: './workerMessage-component.html',
    styleUrls: ['./workerMessage-component.scss']
})

export class workerMessageComponent implements OnInit {

    @Input() workerName: string;
    @Input() rec: any;
    @Input() backgroundColor: string = '#45526e';
    
    msgHeader: string;
    msgRecipient: string;
    msgExpire: string;
    msgTypeColor: string = '#00C851';
    workerColor: string = '#2e3951';
    
    ngOnInit() {
        if (+this.rec.SF_WorkerMessages_MessageType == 2044788736)
        {
            this.msgHeader = 'Hint';
        }
        else if (+this.rec.SF_WorkerMessages_MessageType == 2044788737)
        {
            this.msgHeader = 'Warning';
            this.msgTypeColor = '#ffbb33';
        }
        else if (+this.rec.SF_WorkerMessages_MessageType == 2044788738)
        {
            this.msgHeader = 'Error';
            this.msgTypeColor = '#ff4444';
        }

        if (!this.IsDateEmpty(this.rec.SF_WorkerMessages_MessageDate))
            this.msgHeader += ' [' + this.rec.SFWorkerMessages_MessageDate + ']';

        if (+this.rec.SF_WorkerMessages_WorkerID == 0)
            this.msgRecipient = 'All workers';
        else
        {
            this.msgRecipient = this.workerName;
            this.workerColor = '#CC0000';
        }
        
        if (this.rec.SF_WorkerMessages_Expire == '1')
        {
            if (this.IsDateEmpty(this.rec.SF_WorkerMessages_ExpirationDate))
                this.msgExpire = 'Expire on exit';
            else
                this.msgExpire = 'Expire on ' + this.rec.SF_WorkerMessages_ExpirationDate;
        }
    } 

    IsDateEmpty(d: string) {
        return (d === null || d === '1799-12-31');  
    }
 }
