import React from "react";
import {MemoryRouter, Route} from "react-router-dom";
import {fakeAppContext, fakeTeam} from "../Test/mockData";
import {responseMap} from "../Test/testHelpers";
import {render, unmountComponentAtNode} from "react-dom";
import {HomeContainer} from "./HomeContainer";
import {act} from "react-dom/test-utils";
import AppContext from "../Shared/AppContext";

jest.mock("react-leaflet", () => ({
  MapContainer: () => <div id="MapContainer">MapContainer</div>,
  TileLayer: () => <div id="TileLayer">TileLayer</div>,
  Popup: () => <div id="Popup">Popup</div>,
  GeoJSON: () => <div id="GeoJSON">GeoJSON</div>,
  FeatureGroup: () => <div id="FeatureGroup">FeatureGroup</div>,
}));

let container: Element;

beforeEach(() => {
  (global as any).Harvest = fakeAppContext;

  // setup a DOM element as a render target
  container = document.createElement("div");
  document.body.appendChild(container);

  const teamResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve([fakeTeam]),
  });

  global.fetch = jest.fn().mockImplementation((x) =>
      responseMap(x, {
        "/api/team": teamResponse,
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
});

describe("Home Container", () => {
  it("redirects to the team page", async () => {
    await act(async () => {
      render(
          <AppContext.Provider value={(global as any).Harvest}>
            <MemoryRouter initialEntries={["/"]}>
              <Route path="/">
                <HomeContainer />
              </Route>
              <Route path="/team">
                <div id="team-info">You are on the team page</div>
              </Route>
            </MemoryRouter>
          </AppContext.Provider>,
          container
      );
    });
    
    const roleInfo = container.querySelector("#team-info")?.textContent
    expect(roleInfo).toContain("You are on the team page")
  });
});
