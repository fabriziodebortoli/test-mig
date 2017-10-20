export class Helpers {

    static hasBeenTypedANumber(e: KeyboardEvent): boolean {
        let admittedSpecialKeys: Array<string> = ['Backspace', 'Tab', 'End', 'Home', 'ArrowLeft', 'ArrowRight', 'Escape'];
        let regex: RegExp = new RegExp(/^-?\d+$/);

        if (admittedSpecialKeys.indexOf(e.key) !== -1) return true;
        if (e.ctrlKey === true && (e.key === 'c' || e.key === 'x' || e.key === 'v' || e.key === 'z')) return true;

        const current: string = e.key;
        const next: string = current.concat(e.key);
        if (next && !String(next).match(regex))
            return false;
        return true;
    }

    static hasBeenPastedANumber(e: ClipboardEvent): boolean {
        let regex: RegExp = new RegExp(/^-?\d+$/);
        if (e.type === 'paste') {
            const pasteData = e.clipboardData.getData('text');
            if (pasteData && !String(pasteData).match(regex)) {
                return false;
            }
        }
        return true;
    }
}

