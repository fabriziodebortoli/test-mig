export default class Tax {
    public static isValid(isoCode: string, vat: string) {
        isoCode = isoCode && isoCode.toUpperCase();
        return !Tax['isTaxIdValid' + isoCode] ||
            Tax['isTaxIdValid' + isoCode](vat);
    }

    static isTaxIdValidIT(s: string): boolean {
        return this.isTaxIdValid1(s) || this.isTaxIdValid2(s);
    }

    static isTaxIdValid1(s: string): boolean {
        if (!s) return true;
        if (s.length !== 11) return false;
        const array = s.split('').map(x => x.charCodeAt(0) - 48);
        if (array[10] === 0) return true;
        let odd = 0, even = 0;
        for (let i = 0; i <= 8; i += 2)	odd += array[i];
        for (let i = 1; i <= 9; i += 2) {
            const n = array[i];
            if (n < 5) even += 2 * n;
            else even += 2 * n - 9;
        }
        if (10 - ((even + odd) % 10) === array[10]) return true;
        return false;
    }

    static isTaxIdValid2(s: string): boolean {
        if (!s) return true;
        if (s.length !== 11) return false;
        const array = s.split('').map(x => x.charCodeAt(0) - 48);
        let odd = 0, even = 0;
        for (let i = 0; i <= 8; i += 2)	odd += array[i];
        for (let i = 1; i <= 9; i += 2) {
            const n = array[i];
            if (n < 5) even += 2 * n;
            else even += n * 2 - n * 2 / 10 * 9;
        }
        const last = array[10];
        const top = 10 - ((even + odd) % 10);
        if ((even + odd) % 10 === 0 && last !== 0 ||
            (even + odd) % 10 !== 0 && last !== top)
            return false;
        let sum = 0;
        for (let i = 0; i < 6; i++) sum += array[i];
        return sum !== 0 || 100 * array[7] + 10 * array[8] * array[9] >= 100
    }
}
