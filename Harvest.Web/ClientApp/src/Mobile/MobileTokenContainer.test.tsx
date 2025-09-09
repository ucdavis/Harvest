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

  // Restore original fetch if it was mocked
  if (jest.isMockFunction(global.fetch)) {
    (global.fetch as jest.Mock).mockRestore();
  }
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
  expect(container.textContent).toContain("Authorize Mobile App");
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
  expect(button?.textContent).toContain("Authorize Mobile App");
});

it("shows success message and hides button when authorized", async () => {
  // Mock successful API response
  global.fetch = jest.fn(() =>
    Promise.resolve({
      ok: true,
      json: () => Promise.resolve("fake-token-123"),
    })
  ) as jest.Mock;

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

  // Click the authorize button
  const button = container.querySelector("button");
  expect(button).not.toBeNull();

  button?.click();

  // Wait for the async operation to complete
  await act(async () => {
    await new Promise((resolve) => setTimeout(resolve, 0));
  });

  // Check that success message appears
  expect(container.textContent).toContain("Mobile App Authorized!");
  expect(container.textContent).toContain("You can now close this page");

  // Check that button is no longer visible
  const buttonAfter = container.querySelector("button");
  expect(buttonAfter).toBeNull();
});

it("shows error message when authorization fails", async () => {
  // Mock failed API response
  global.fetch = jest.fn(() =>
    Promise.resolve({
      ok: false,
      text: () => Promise.resolve("Authorization failed"),
    })
  ) as jest.Mock;

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

  // Click the authorize button
  const button = container.querySelector("button");
  expect(button).not.toBeNull();

  button?.click();

  // Wait for the async operation to complete
  await act(async () => {
    await new Promise((resolve) => setTimeout(resolve, 0));
  });

  // Check that error message appears
  expect(container.textContent).toContain("Error:");
  expect(container.textContent).toContain("Authorization failed");

  // Check that button is still visible for retry
  const buttonAfter = container.querySelector("button");
  expect(buttonAfter).not.toBeNull();
});
