import { Align } from '@progress/kendo-angular-popup';

export class PopupHelper {
    public static getPopupAlign(anchor: any): Align {
        let Horizontal = 'left';
        let Vertical = 'top';
    
        let height = window.innerHeight;
        let bottomPoint = anchor.offsetTop + anchor.offsetHeight;
        if (height - bottomPoint <= 300) {
          Vertical = 'bottom';
        }
    
        let width = window.innerWidth;
        let rightPoint = anchor.offsetLeft + anchor.offsetWidth;
        if (rightPoint > (width *0.45)) {
          Horizontal = 'right';
        }
        return {
          horizontal: Horizontal,
          vertical: Vertical
        } as Align;
      }
    
    public static getAnchorAlign(anchor: any): Align {
        let Horizontal = 'left';
        let Vertical = 'bottom';
    
        let height = window.innerHeight;
        let bottomPoint = anchor.offsetTop + anchor.offsetHeight;
        if (height - bottomPoint <= 300) {
          Vertical = 'top';
        }
    
        let width = window.innerWidth;
        let rightPoint = anchor.offsetLeft + anchor.offsetWidth;
        if (rightPoint > (width *0.45)) {
          Horizontal = 'right';
        }
    
        return {
          horizontal: Horizontal,
          vertical: Vertical
        } as Align;
      }
}