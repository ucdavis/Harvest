import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";

import { QuoteContainer } from "./QuoteContainer";
import {
  fakeAppContext,
  fakeProjectWithQuote,
  sampleRates,
} from "../Test/mockData";
import { ValidationProvider } from "use-input-validator";

let container: Element;

jest.mock("react-leaflet", () => ({
  MapContainer: () => <div id="MapContainer">MapContainer</div>,
  TileLayer: () => <div id="TileLayer">TileLayer</div>,
  Popup: () => <div id="Popup">Popup</div>,
  GeoJSON: () => <div id="GeoJSON">GeoJSON</div>,
  FeatureGroup: () => <div id="FeatureGroup">FeatureGroup</div>,
}));

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

describe("Quote Container", () => {
  it("Shows loading screen", async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={["/quote/create/1"]}>
          <Route path="/quote/create/:projectId">
            <ValidationProvider>
              <QuoteContainer />
            </ValidationProvider>
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
        json: () => Promise.resolve(fakeProjectWithQuote),
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
            <ValidationProvider>
              <QuoteContainer />
            </ValidationProvider>
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const messageContent =
      document.querySelector("#request-title")?.textContent;
    expect(messageContent).toContain("Field Request #3");
  });
});
