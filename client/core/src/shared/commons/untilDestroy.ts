import { Observable } from 'rxjs/Observable';
import { takeUntil } from 'rxjs/operators';

export const destroy$ = Symbol('destroy$');

/**
 * Completes the source Observable when component gets destroyed (ngOnDestroy)
 * @param component the component on which to listen for ngOnDestory
 */
export const untilDestroy = component => <T>(source: Observable<T>) => {
  if (component[destroy$] === undefined)
    addDestroyObservableToComponent(component);
  return source.pipe(takeUntil(component[destroy$]));
};

export function addDestroyObservableToComponent(component) {
  component[destroy$] = new Observable<void>(observer => {
    const orignalDestroy = component.ngOnDestroy;
    if (orignalDestroy === undefined) { // Angular does not support dynamic added destroy methods, so make sure there is one
      throw new Error(
        'untilDestroy operator needs the component to have an ngOnDestroy method'
      );
    }
    component.ngOnDestroy = () => {
      observer.next();
      observer.complete();
      orignalDestroy.call(component);
    };
    return _ => (component[destroy$] = undefined);
  });
}
