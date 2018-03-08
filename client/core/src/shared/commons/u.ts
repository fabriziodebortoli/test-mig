export function tryOrDefault<T>(f: () => T, def?: T): T {
  try {
      return f();
  } catch (e) {
      console.error(e);
      return def;
  }
}

export function findAnchestorByClass(el: any, cls: string): any {
    if(!el) return null;
    while ((el = el.parentElement) && !el.classList.contains(cls));
    return el;
}
