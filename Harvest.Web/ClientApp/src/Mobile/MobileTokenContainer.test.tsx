import React from "react";
import { MemoryRouter, Route } from "react-router-dom";
import { fakeAppContext } from "../Test/mockData";
import { render, unmountComponentAtNode } from "react-dom";
import { MobileTokenContainer } from "./MobileTokenContainer";
import { act } from "react-dom/test-utils";
import AppContext from "../Shared/AppContext";

let container: Element;

beforeEach(() => {
  (global as any).Harvest = fakeAppContext;

  // setup a DOM element as a render target
  container = document.createElement("div");
  document.body.appendChild(container);
});

afterEach(() => {
  // cleanup on exiting
  unmountComponentAtNode(container);
  document.body.removeChild(container);
});

it("renders mobile token container", async () => {
  await act(async () => {
    render(
      <AppContext.Provider value={fakeAppContext}>
        <MemoryRouter initialEntries={["/fakeTeam/mobile/token"]}>
          <Route path="/:team/mobile/token" component={MobileTokenContainer} />
        </MemoryRouter>
      </AppContext.Provider>,
      container
    );
  });

  expect(container.textContent).toContain("Mobile Token Generator");
  expect(container.textContent).toContain("Generate Mobile Token");
});

it("displays generate button", async () => {
  await act(async () => {
    render(
      <AppContext.Provider value={fakeAppContext}>
        <MemoryRouter initialEntries={["/fakeTeam/mobile/token"]}>
          <Route path="/:team/mobile/token" component={MobileTokenContainer} />
        </MemoryRouter>
      </AppContext.Provider>,
      container
    );
  });

  const button = container.querySelector("button");
  expect(button).not.toBeNull();
  expect(button?.textContent).toContain("Generate Mobile Token");
});
