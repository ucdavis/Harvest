export function groupBy<T extends any, K extends keyof T>(
  array: T[],
  key: K | { (obj: T): string }
): T[][] {
  const keyFn = key instanceof Function ? key : (obj: T) => obj[key as K];
  const records = array.reduce((objectsByKeyValue, obj) => {
    const value = String(keyFn(obj));
    objectsByKeyValue[value] = (objectsByKeyValue[value] || []).concat(obj);
    return objectsByKeyValue;
  }, {} as Record<string, T[]>);

  return Object.values(records);
}
