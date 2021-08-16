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
