import { calculateAdjustedTotal, calculateMarkupAmount } from "./Calculations";
import { WorkItemImpl } from "../types";

describe("calculateAdjustedTotal", () => {
  it("adds 20 percent markup when the expense total is below the cap", () => {
    const workItem = new WorkItemImpl(1, 1, "Other");
    workItem.rate = 450;
    workItem.quantity = 1;
    workItem.markup = true;

    expect(calculateAdjustedTotal(workItem, 0)).toBe(540);
  });

  it("applies the 200 dollar markup when the base total is exactly at the cap", () => {
    const workItem = new WorkItemImpl(1, 1, "Other");
    workItem.rate = 1000;
    workItem.quantity = 1;
    workItem.markup = true;

    expect(calculateAdjustedTotal(workItem, 0)).toBe(1200);
  });

  it("caps markup at 200 dollars per expense", () => {
    const workItem = new WorkItemImpl(1, 1, "Other");
    workItem.rate = 900;
    workItem.quantity = 10;
    workItem.markup = true;

    expect(calculateAdjustedTotal(workItem, 0)).toBe(9200);
  });

  it("applies the cap after annual adjustment changes the base total", () => {
    const workItem = new WorkItemImpl(1, 1, "Other");
    workItem.rate = 100;
    workItem.quantity = 5;
    workItem.markup = true;

    expect(calculateAdjustedTotal(workItem, 10)).toBe(660);
  });

  it("rounds fractional-cent markup and adjusted totals to two decimals", () => {
    const workItem = new WorkItemImpl(1, 1, "Other");
    workItem.rate = 16.675;
    workItem.quantity = 1;
    workItem.markup = true;

    expect(calculateMarkupAmount(workItem.rate * workItem.quantity, true)).toBe(
      3.34
    );
    expect(calculateAdjustedTotal(workItem, 0)).toBe(20.02);
  });
});

describe("calculateMarkupAmount", () => {
  it("returns zero when markup is not selected", () => {
    expect(calculateMarkupAmount(1000, false)).toBe(0);
  });
});
