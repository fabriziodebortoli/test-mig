export class StringUtils {
    // object.padding(number, string)
    // Transform the string object to string of the actual width filling by the padding character (by default ' ')
    // Negative value of width means left padding, and positive value means right one
    static pad(val: string, n: number): string {
        if (Math.abs(n) <= val.length) return val;
        let m = Math.max((Math.abs(n) - val.length) || 0, 0);
        let pad = Array(m + 1).join(String(' ').charAt(0));
        return (n < 0) ? pad + val : val + pad;
    };
}
