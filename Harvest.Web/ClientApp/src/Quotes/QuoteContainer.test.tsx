import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";

import { QuoteContainer } from "./QuoteContainer";
import { fakeProject, sampleRates } from "../Test/mockData";

let container: Element;

beforeEach(() => {
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

describe("Quote Container", () => {
  it("Shows loading screen", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={["/quote/create/1"]}>
          <Route path="/quote/create/:projectId">
            <QuoteContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const messageContent = container.querySelector("div")?.textContent;
    expect(messageContent).toContain("Loading");
  });
  it("loads field request", async () => {
    await act(async () => {
      const response = {
        status: 200,
        ok: true,
        json: () => Promise.resolve(fakeProject),
      };

      const response2 = {
        status: 200,
        ok: true,
        json: () => Promise.resolve(sampleRates),
      };

      global.fetch = jest
        .fn()
        .mockImplementationOnce(() => Promise.resolve(response)) // return project info
        .mockImplementationOnce(() => Promise.resolve(response2)); // return rate info

      render(
        <MemoryRouter initialEntries={["/quote/create/3"]}>
          <Route path="/quote/create/:projectId">
            <QuoteContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const messageContent = container.querySelector("#request-title")
      ?.textContent;
    expect(messageContent).toContain("Field Request #3");
  });
});
