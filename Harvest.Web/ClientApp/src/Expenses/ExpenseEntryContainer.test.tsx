import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act, Simulate } from "react-dom/test-utils";

import { ExpenseEntryContainer } from "./ExpenseEntryContainer";
import { fakeAppContext, sampleRates, fakeProject } from "../Test/mockData";
import { responseMap } from "../Test/testHelpers";

let container: Element;

beforeEach(() => {
  const projectResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeProject),
  });
  const rateResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(sampleRates),
  });

  (global as any).Harvest = fakeAppContext;
  // setup a DOM element as a render target
  container = document.createElement("div");
  document.body.appendChild(container);

  global.fetch = jest.fn().mockImplementation((x) =>
    responseMap(x, {
      "/api/Project": projectResponse,
      "/api/Rate/Active": rateResponse,
    })
  );
});

afterEach(() => {
  // cleanup on exiting
  // clear any mocks living on fetch
  if ((global.fetch as any).mockClear) {
    (global.fetch as any).mockClear();
  }
  unmountComponentAtNode(container);
  container.remove();
  // container = null;
});

describe("Expense Entry Container", () => {
  it("Generic Activity Default", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={["/expense/entry/3"]}>
          <Route path="/expense/entry/:projectId">
            <ExpenseEntryContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const defaultActivity = container.querySelector(
      ".card-wrapper.mb-4.no-green"
    );
    expect(defaultActivity?.textContent).toContain("Activity Name");
  });

  it("Rate Reflection", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={["/expense/entry/3"]}>
          <Route path="/expense/entry/:projectId">
            <ExpenseEntryContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const dropDown = document.querySelector(
      ".rbt-input-main.form-control.rbt-input"
    ) as HTMLInputElement;
    Simulate.click(dropDown);
    const link = document.querySelector(
      "#typeahead-Labor-item-0"
    ) as HTMLElement;
    Simulate.click(link);
    const price = container.querySelector(".rate-3") as HTMLElement;

    expect(price?.textContent).toContain("$60.00");
  });

  it("Total Value", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={["/expense/entry/3"]}>
          <Route path="/expense/entry/:projectId">
            <ExpenseEntryContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const dropDown = document.querySelector(
      ".rbt-input-main.form-control.rbt-input"
    ) as HTMLInputElement;
    Simulate.click(dropDown);
    const link = document.querySelector(
      "#typeahead-Labor-item-0"
    ) as HTMLElement;
    Simulate.click(link);

    const unitsInput = container.querySelector("#units") as HTMLElement;
    Simulate.change(unitsInput, { target: { value: "3" } });
    const total = container.querySelector(".total-3");

    expect(total?.textContent).toContain("$180.00");
  });
});
