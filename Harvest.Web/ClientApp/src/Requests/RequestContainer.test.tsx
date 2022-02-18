import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";

import { RequestContainer } from "./RequestContainer";
import { fakeAppContext, fakeCrops, fakeProject } from "../Test/mockData";
import { responseMap } from "../Test/testHelpers";
import "jest-canvas-mock";

let container: Element;

beforeEach(() => {
  const projectResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeProject),
  });

  const fileResponse = Promise.resolve({
    status: 200,
    ok: true,
    text: () => Promise.resolve("file 1"),
  });

  const cropResponse = Promise.resolve({
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeCrops.filter((c) => c.type === "Row")),
  });

  (global as any).Harvest = fakeAppContext;
  // setup a DOM element as a render target
  container = document.createElement("div");
  document.body.appendChild(container);
  global.fetch = jest.fn().mockImplementation((x) =>
    responseMap(x, {
      "/api/Project": projectResponse,
      "/api/File/": fileResponse,
      "/api/Crop": cropResponse,
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
  it("Populate form", async () => {
    await act(async () => {
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
      "react-datepicker__input-container"
    );
    const startDate = dates[0].querySelector("input");
    const endDate = dates[1].querySelector("input");
    const cropType = container.querySelectorAll(
      ".custom-control.custom-radio"
    )[0];
    const vegetable = container.querySelector(".rbt-input-wrapper");
    const PI = container.querySelectorAll("input.rbt-input-main");

    expect(startDate?.value).toContain("03/15/2021");
    expect(endDate?.value).toContain("03/29/2021");
    expect(cropType?.innerHTML).toContain("checked");
    expect(vegetable?.textContent).toContain("Tomato");
    expect(PI[1]?.value).toContain(
      "Mr Mr Mr Bob Dobalina (bdobalina@ucdavis.edu)"
    );
  });

  it("No Project", async () => {
    await act(async () => {
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
      "react-datepicker__input-container"
    );
    const startDate = dates[0].querySelector("input");
    const endDate = dates[1].querySelector("input");
    const cropType = container.querySelectorAll(
      ".custom-control.custom-radio"
    )[0];
    const vegetable = container.querySelector(".rbt-input-wrapper");
    const PI = container.querySelectorAll("input.rbt-input-main");
    const button = container.querySelector("#Request-Button");

    expect(startDate?.value).toContain("");
    expect(endDate?.value).toContain("");
    expect(cropType?.innerHTML).toContain("");
    expect(vegetable?.textContent).toContain("");
    expect(PI[1]?.value).toContain("");
    expect(button).toContainHTML("disabled=");
  });
});
