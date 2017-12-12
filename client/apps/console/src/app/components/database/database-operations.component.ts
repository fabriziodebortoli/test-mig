import { Component, OnInit, Input } from '@angular/core';
import { OperationResult } from '../../services/operationResult';
import { ModelService } from 'app/services/model.service';
import { Router } from '@angular/router';
import { MessageData } from '../../services/messageData';
import { Subject } from 'rxjs';
import { ExtendedSubscriptionDatabase } from './helpers/database-helpers';

@Component({
  selector: 'app-database-operations',
  templateUrl: './database-operations.component.html',
  styleUrls: ['./database-operations.component.css']
})

export class DatabaseOperationsComponent implements OnInit {

  @Input() operations: OperationResult[];
  @Input() readingData: boolean;
  @Input() operationsToDo: boolean;
  @Input() extSubDatabase: ExtendedSubscriptionDatabase;

  isWorking: boolean;

  //-----------------------------------------------------------------------------	
  constructor(private modelService: ModelService, private router: Router) {
    this.operations = [];
  }

  //-----------------------------------------------------------------------------	
  ngOnInit() {
    this.isWorking = false;
  }

  //-----------------------------------------------------------------------------	
  confirmDbOperations() {

    if (this.extSubDatabase === undefined) {
      alert('Invalid data');
      return;
    }

    this.isWorking = true;

    let test = this.modelService.testConnection(this.extSubDatabase.Database.SubscriptionKey, this.extSubDatabase.AdminCredentials).
      subscribe(
      testResult => {

        if (testResult.Result) {
          let update = this.modelService.updateDatabase(this.extSubDatabase.Database.SubscriptionKey, this.extSubDatabase).
            subscribe(
            updateResult => {

              // sample to send email after update database
              /*let messageData: MessageData = new MessageData();
              messageData.Destination = 'michela.delbene@microarea.it';
              messageData.Subject = 'Update subscription database ' + this.extSubDatabase.Database.Name 
              + ' for Subscription ' + this.extSubDatabase.Database.SubscriptionKey;
              messageData.Body = updateResult.Message;

              // I send an email
              let sendMessage = this.modelService.sendMessage(messageData).
                subscribe(
                sendResult => {
                  sendMessage.unsubscribe();
                },
                sendError => {
                  console.log(sendError);
                  alert(sendError);
                  sendMessage.unsubscribe();
                }
                )*/

              if (!updateResult.Result) {
                alert(updateResult.Message);
              }
              else
              {
                // if everything is ok I return to subscription home page
                this.router.navigate(['/subscription'], { queryParams: { subscriptionToEdit: this.extSubDatabase.Database.SubscriptionKey } });
              }
              update.unsubscribe();
            },
            updateError => {
              console.log(updateError);
              alert(updateError);
              update.unsubscribe();
            }
            )
        }
        else
          alert('Unable to connect! ' + testResult.Message);

        test.unsubscribe();
      },
      error => {
        console.log(error);
        alert(error);
        test.unsubscribe();
      }
      );
  }
}
