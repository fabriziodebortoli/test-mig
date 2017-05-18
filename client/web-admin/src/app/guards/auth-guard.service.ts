import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild } from '@angular/router';

@Injectable()
export class AuthGuardService implements CanActivate {

  constructor() { }

  canActivate() {
    return false;
  }

  canActivateChild() {
    return false;
  }  
}
