import { ModelService } from './services/model.service';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})

export class AppComponent implements OnInit {

  constructor(private router: Router) {}

  ngOnInit() {
    this.router.navigateByUrl('/appHome');
  }  
}
