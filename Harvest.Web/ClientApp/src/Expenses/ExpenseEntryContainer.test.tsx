import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";

import { ExpenseEntryContainer } from "./ExpenseEntryContainer";
import { fakeAppContext, fakeProjectWithQuote, sampleRates } from "../Test/mockData";

let container: Element;

beforeEach(() => {
  (global as any).Harvest = fakeAppContext;
  // setup a DOM element as a render target
  container = document.createElement("div");
  document.body.appendChild(container);
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

    const defaultActivity = container.querySelector(".card-wrapper.mb-4.no-green");
    expect(defaultActivity.textContent).toContain("Activity Name");
  });
});
