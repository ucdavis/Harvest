import { WorkItem } from "../types";

export const calculateAdjustedTotal = (
  workItem: WorkItem,
  adjustment: number
) => {
  const markup = workItem.markup ? 1.2 : 1;
  return (
    (workItem.rate + (workItem.rate * adjustment) / 100.0) *
    workItem.quantity *
    markup
  );
};

export const roundToTwo = (num: number) => {
  // shift decimal right two places, round, then shift left two places
  return Math.round(num * 1e2) * 1e-2;
};

export const countDecimals = (num: number) => {
  if (Math.floor(num.valueOf()) === num.valueOf()) return 0;

  // Handle edge cases where the number is very small
  var str = num.toString();
  if (str.indexOf(".") !== -1 && str.indexOf("e-") !== -1) {
    // 1.1e-7 (0.00000011) will return 8
    const values = str.split("e-");
    return (+values[1] || 0) + (values[0].split(".")[1].length || 0);
  } else if (str.indexOf(".") !== -1) {
    // 0.000001 will return 6
    return str.split(".")[1].length;
  }
  // 1e-7 (0.0000001) will return 7
  return +str.split("e-")[1];
};

// milliseconds per second per minute per hour per day (what about DST, leap year/second? Pft!)
export const millisecondsPerHour = 1000 * 60 * 60;
export const millisecondsPerDay = millisecondsPerHour * 24;

export const getDaysDiff = (date1: Date, date2: Date) =>
  (date1.getTime() - date2.getTime()) / millisecondsPerDay;
export const getHoursDiff = (date1: Date, date2: Date) =>
  (date1.getTime() - date2.getTime()) / millisecondsPerHour;
export const addDays = (date: Date, days: number) =>
  new Date(date.getTime() + days * millisecondsPerDay);
