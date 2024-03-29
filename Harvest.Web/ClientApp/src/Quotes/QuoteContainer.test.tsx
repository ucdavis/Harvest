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
import { responseMap } from "../Test/testHelpers";
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
  const quoteResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeProjectWithQuote),
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
      "/api/team1/Quote/Get/": quoteResponse,
      "/api/team1/Rate/Active": rateResponse,
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

describe("Quote Container", () => {
  it("Shows loading screen", async () => {
    const notOkProjectResponse = Promise.resolve({
      status: 200,
      ok: false,
      json: () => Promise.resolve(fakeProjectWithQuote),
    });
    const notOkRateResponse = Promise.resolve({
      status: 200,
      ok: false,
      json: () => Promise.resolve(sampleRates),
    });

    await act(async () => {
      global.fetch = jest.fn().mockImplementation((x) =>
        responseMap(x, {
          "/api/team1/Quote/Get/": notOkProjectResponse,
          "/api/team1/Rate/Active": notOkRateResponse,
        })
      );

      render(
        <MemoryRouter initialEntries={["/team1/quote/create/1"]}>
          <Route path="/:team/quote/create/:projectId">
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
      render(
        <MemoryRouter initialEntries={["/team1/quote/create/3"]}>
          <Route path="/:team/quote/create/:projectId">
            <QuoteContainer />
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
