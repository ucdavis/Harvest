import { WorkItem } from "../types";

export const expenseMarkupRate = 0.2;
export const expenseMarkupCap = 1000;

export const calculateMarkupAmount = (
  baseTotal: number,
  applyMarkup: boolean
) => {
  if (!applyMarkup || baseTotal <= 0) {
    return 0;
  }

  return Math.min(baseTotal, expenseMarkupCap) * expenseMarkupRate;
};

export const calculateAdjustedTotal = (
  workItem: WorkItem,
  adjustment: number
) => {
  const baseTotal =
    (workItem.rate + (workItem.rate * adjustment) / 100.0) *
    workItem.quantity;

  return baseTotal + calculateMarkupAmount(baseTotal, workItem.markup);
};

export const roundToTwo = (num: number) => {
  // shift decimal right two places, round, then shift left two places
  return Math.round(num * 1e2) * 1e-2;
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
