import { StateButtonComponent } from './../controls/state-button/state-button.component';
import { Directive,  AfterContentInit, ViewContainerRef, ComponentFactoryResolver, ComponentRef } from '@angular/core';


@Directive({
  selector: '[tbStateButtons]'
})
export class StateButtonDirective implements AfterContentInit {
private stateButtonsRef: ComponentRef<any>;
  constructor(private vcr: ViewContainerRef, private componentResolver: ComponentFactoryResolver) { 
      console.log(vcr);
  }
 renderComponent() {
    // if (this.stateButtonsRef) this.stateButtonsRef.instance.value = this.value;
  }
ngAfterContentInit() {

    this.stateButtonsTarget = (<any>this.vcr)._data.componentView.component.stateButtons;
    console.log('_data.componentView', this.stateButtonsTarget);

    let componentFactory = this.componentResolver.resolveComponentFactory(StateButtonComponent);
    this.stateButtonsRef = this.stateButtonsTarget.createComponent(componentFactory);
    this.renderComponent();
  }

  ngOnChanges(changes: Object) {
    this.renderComponent();
  }
}
