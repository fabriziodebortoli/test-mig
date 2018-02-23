export function tryOrDefault<T>(f: () => T, def?: T): T {
  try {
      return f();
  } catch (e) {
      console.error(e);
      return def;
  }
}
