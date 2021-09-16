import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";

import { RequestContainer } from "./RequestContainer";
import { fakeAppContext, fakeCrops, fakeProject } from "../Test/mockData";
import "jest-canvas-mock";

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

describe("Request Container", () => {
  const projectResponse = {
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeProject),
  };

  const fileResponse = {
    status: 200,
    ok: true,
    text: () => Promise.resolve("file 1"),
  };

  const cropResponse = {
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeCrops.filter((c) => c.type === "Row")),
  };

  it("Populate form", async () => {
    await act(async () => {
      global.fetch = jest
        .fn()
        .mockImplementationOnce(() => Promise.resolve(projectResponse))
        .mockImplementationOnce(() => Promise.resolve(cropResponse))
        .mockImplementationOnce(() => Promise.resolve(fileResponse));

      render(
        <MemoryRouter initialEntries={["/request/create/3"]}>
          <Route path="/request/create/:projectId?">
            <RequestContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const dates = container.getElementsByClassName(
      "react-date-picker__inputGroup"
    );
    const startDate = dates[0].querySelector("input");
    const endDate = dates[1].querySelector("input");
    const cropType = container.querySelectorAll(
      ".custom-control.custom-radio"
    )[0];
    const vegetable = container.querySelector(".rbt-input-wrapper");
    const PI = container.querySelectorAll("input.rbt-input-main");

    expect(startDate?.value).toContain("2021-03-15");
    expect(endDate?.value).toContain("2021-03-29");
    expect(cropType?.innerHTML).toContain("checked");
    expect(vegetable?.textContent).toContain("Tomato");
    expect(PI[1]?.value).toContain(
      "Mr Mr Mr Bob Dobalina (bdobalina@ucdavis.edu)"
    );
  });

  it("No Project", async () => {
    await act(async () => {
      global.fetch = jest
        .fn()
        .mockImplementationOnce(() => Promise.resolve(cropResponse))
        .mockImplementationOnce(() => Promise.resolve(fileResponse));

      render(
        <MemoryRouter initialEntries={["/request/create"]}>
          <Route path="/request/create/:projectId?">
            <RequestContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const dates = container.getElementsByClassName(
      "react-date-picker__inputGroup"
    );
    const startDate = dates[0].querySelector("input");
    const endDate = dates[1].querySelector("input");
    const cropType = container.querySelectorAll(
      ".custom-control.custom-radio"
    )[0];
    const vegetable = container.querySelector(".rbt-input-wrapper");
    const PI = container.querySelectorAll("input.rbt-input-main");
    const button = container.querySelectorAll(".row.justify-content-center")[1];

    expect(startDate?.value).toContain("");
    expect(endDate?.value).toContain("");
    expect(cropType?.innerHTML).toContain("");
    expect(vegetable?.textContent).toContain("");
    expect(PI[1]?.value).toContain("");
    expect(button.innerHTML).toContain("disabled");
  });
});
