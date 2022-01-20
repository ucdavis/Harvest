import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";
import AppContext from "../Shared/AppContext";

import { TicketsContainer } from "./TicketsContainer";

import { fakeAppContext, fakeTickets, fakeProject } from "../Test/mockData";

import { responseMap } from "../Test/testHelpers";

import "jest-canvas-mock";

let container: Element;

beforeEach(() => {
  const projectResponse = {
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeProject),
  };

  const ticketResponses = {
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeTickets),
  };

  (global as any).Harvest = fakeAppContext;
  // setup a DOM element as a render target
  container = document.createElement("div");
  document.body.appendChild(container);

  global.fetch = jest.fn().mockImplementation((x) =>
    responseMap(x, {
      "/api/Project/Get/": projectResponse,
      "/api/Ticket/GetList": ticketResponses,
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

describe("Request Container", () => {
  it("Shows loading screen", async () => {
    const notOkProjectResponse = {
      status: 200,
      ok: false,
      json: () => Promise.resolve(undefined),
    };
    const notOkticketResponses = {
      status: 200,
      ok: true,
      json: () => Promise.resolve(undefined),
    };

    await act(async () => {
      //Clear out the fetch mock and return a not ok for the loading text
      global.fetch = jest.fn().mockImplementation((x) =>
        responseMap(x, {
          "/api/Project/Get/": notOkProjectResponse,
          "/api/Ticket/GetList": notOkticketResponses,
        })
      );

      render(
        <AppContext.Provider value={(global as any).Harvest}>
          <MemoryRouter initialEntries={["/ticket/List/3"]}>
            <Route path="/ticket/List/:projectId">
              <TicketsContainer />
            </Route>
          </MemoryRouter>
        </AppContext.Provider>,
        container
      );
    });

    const messageContent = container.querySelector("div")?.textContent;
    expect(messageContent).toContain("Loading");
  });
});
