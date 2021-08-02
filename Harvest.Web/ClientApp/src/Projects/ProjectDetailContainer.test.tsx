import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";
import AppContext from "../Shared/AppContext";

import { ProjectDetailContainer } from "./ProjectDetailContainer";
import {
  fakeAppContext,
  fakeInvoices,
  fakeProject,
  fakeTickets,
} from "../Test/mockData";

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
});

describe("Project Detail Container", () => {
  it("Shows loading screen", async () => {
    await act(async () => {
      render(
        <AppContext.Provider value={(global as any).Harvest}>
          <MemoryRouter initialEntries={["/project/details/3"]}>
            <Route path="/project/details/:projectId">
              <ProjectDetailContainer />
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
