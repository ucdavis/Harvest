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
}

// milliseconds per second per minute per hour per day (what about DST, leap year/second? Pft!)
export const millisecondsPerDay = 1000 * 60 * 60 * 24;
