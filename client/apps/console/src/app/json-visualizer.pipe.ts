import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'jsonVisualizer'
})
export class JsonVisualizerPipe implements PipeTransform {

  transform(value: any, args?: any): any {
    return JSON.stringify(value, null, 2)
      .replace(' ', '&nbsp;')
      .replace('\n', '<br/>');
  }
}
