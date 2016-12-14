import { Router } from '@angular/router';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './home.component.html',
  styleUrls: ['home.component.css']
})
export class HomeComponent {

  constructor(private router: Router) { }

  go(namespace: string) {
    this.router.navigate(['/report', namespace]);
  }
}
