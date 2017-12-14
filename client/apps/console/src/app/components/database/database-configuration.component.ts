import { Router, ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { ModelService } from 'app/services/model.service';

@Component({
  selector: 'app-database-configuration',
  templateUrl: './database-configuration.component.html',
  styleUrls: ['./database-configuration.component.css']
})

export class DatabaseConfigurationComponent implements OnInit {
  
  isWorking: boolean;
  subscriptionKey: string;

  //--------------------------------------------------------------------------------------------------------
  constructor(private modelService: ModelService, private router: Router, private route: ActivatedRoute) {
   }
  
  //--------------------------------------------------------------------------------------------------------
  ngOnInit() {
    this.isWorking = false;
    this.subscriptionKey = this.route.snapshot.queryParams['subscriptionToEdit'];
  }
  
  //--------------------------------------------------------------------------------------------------------
  configureDatabase(automatic: boolean) {
    
    // if I choose automatic configuration I open the summary component
    // otherwise I open the database component
    this.router.navigate(automatic ? ['/database/summary'] : ['/database'], { queryParamsHandling: "preserve" } );
  }
}
