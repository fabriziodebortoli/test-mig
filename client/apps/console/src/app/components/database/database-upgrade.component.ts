import { Component, OnInit, Input } from '@angular/core';
import { SubscriptionDatabase } from 'app/model/subscriptionDatabase';
import { ModelService } from '../../services/model.service';

@Component({
  selector: 'app-database-upgrade',
  templateUrl: './database-upgrade.component.html',
  styleUrls: ['./database-upgrade.component.css']
})

export class DatabaseUpgradeComponent implements OnInit {

  @Input() model: SubscriptionDatabase;
  
  //-----------------------------------------------------------------------------	
  constructor(private modelService: ModelService) {
   }

  //-----------------------------------------------------------------------------	
  ngOnInit() {
    
  }
}
