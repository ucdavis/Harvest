import React from "react";
import { render, screen } from "@testing-library/react";
import { BrowserRouter as Router } from "react-router-dom";
import App from "./App";
import { fakeAppContext } from "./Test/mockData";

jest.mock("react-leaflet", () => ({
  MapContainer: () => <div id="MapContainer">MapContainer</div>,
  TileLayer: () => <div id="TileLayer">TileLayer</div>,
  Popup: () => <div id="Popup">Popup</div>,
  GeoJSON: () => <div id="GeoJSON">GeoJSON</div>,
  FeatureGroup: () => <div id="FeatureGroup">FeatureGroup</div>,
}));

beforeEach(() => {
  (global as any).Harvest = fakeAppContext;
});

describe("Request Container", () => {
  it("Populate form", async () => {
    render(
      <Router>
        <App />
      </Router>
    );
    const linkElement = screen.getByText(/You have the following roles:/i);
    expect(linkElement).toBeInTheDocument();
  })
})
