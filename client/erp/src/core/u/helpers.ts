const admittedSpecialKeys: Array<string> = ['Backspace', 'Tab', 'End', 'Home', 'ArrowLeft', 'ArrowRight', 'Escape'];

function _isNumber(s: string): boolean {
    const regex: RegExp = new RegExp(/^-?\d+$/);
    if (!String(s).match(regex)) return false;
    return true;
}

function _isLetter(s: string): boolean {
    const regex: RegExp = new RegExp(/[a-zA-Z]/);
    if (!String(s).match(regex)) return false;
    return true;
}

function _isNotAlphanumeric(s: string): boolean {
    const regex: RegExp = new RegExp(/[^a-zA-Z\d]/);
    if (!String(s).match(regex)) return false;
    return true;
}

export class KeyboardEventHelper {
    private static isSpecial(e: { key: string, ctrlKey: boolean }): boolean {
        if (admittedSpecialKeys.indexOf(e.key) !== -1) return true;
        if (e.ctrlKey === true && (e.key === 'c' || e.key === 'x' || e.key === 'v' || e.key === 'z')) return true;
        return false;
    }

    static isNumber(e: { key: string, ctrlKey: boolean }): boolean {
        if (this.isSpecial(e)) return true;
        const current: string = e.key;
        const next: string = current.concat(e.key);
        if (next && !_isNumber(next))
            return false;
        return true;
    }

    static isLetter(e: { key: string, ctrlKey: boolean }): boolean {
        if (this.isSpecial(e)) return true;
        const current: string = e.key;
        const next: string = current.concat(e.key);
        if (next && !_isLetter(next))
            return false;
        return true;
    }

    static isNotAlphanumeric(e: { key: string, ctrlKey: boolean }): boolean {
        if (this.isSpecial(e)) return true;
        const current: string = e.key;
        const next: string = current.concat(e.key);
        if (next && !_isNotAlphanumeric(next))
            return false;
        return true;
    }
}

export class ClipboardEventHelper {
    static isNumber(e: {type: string, clipboardData: {getData: (string) => string}}): boolean {
        const pasteData = e.clipboardData.getData('text');
        if (pasteData && !_isNumber(pasteData)) {
            return false;
        }
    }

    static isLetter(e: { clipboardData: { getData: (string) => string } }): boolean {
        const pasteData = e.clipboardData.getData('text');
        if (pasteData && !_isLetter(pasteData)) {
            return false;
        }
    }

    static isNotAlphanumeric(e: { clipboardData: { getData: (string) => string } }): boolean {
        const pasteData = e.clipboardData.getData('text');
        if (pasteData && !_isNotAlphanumeric(pasteData)) {
            return false;
        }
    }
}

