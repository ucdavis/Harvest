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
import { responseMap } from "../Test/testHelpers";

let container: Element;

beforeEach(() => {
  const projectResponse = {
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeProject),
  };

  const unbilledResponse = {
    status: 200,
    ok: true,
    text: () => Promise.resolve("0.00"),
  };

  const fileResponse = {
    status: 200,
    ok: true,
    text: () => Promise.resolve("file 1"),
  };

  const invoiceResponse = {
    status: 200,
    ok: true,
    json: () => Promise.resolve(fakeInvoices),
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
      "/api/Invoice/List/": invoiceResponse,
      "/api/Ticket/GetList": ticketResponses,
      "/api/expense/getunbilledtotal/": unbilledResponse,
      "/api/File/GetUploadDetails": fileResponse,
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

describe("Project Detail Container", () => {
  it("Shows loading screen", async () => {
    const notOkProjectResponse = {
      status: 200,
      ok: false,
      json: () => Promise.resolve(fakeProject),
    };

    await act(async () => {
      //Clear out the fetch mock and return a not ok for the loading text
      global.fetch = jest.fn().mockImplementation((x) =>
        responseMap(x, {
          "/api/Project/Get/": notOkProjectResponse,
        })
      );

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

  it("Load details", async () => {
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

    const fieldTitle = container.querySelector("#request-title")?.textContent;
    expect(fieldTitle).toContain("Field Request #3");
  });

  it("Display correct number of attachments", async () => {
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

    const attachedFilesList = container.querySelector(".attached-files-list");
    const attachemntsLength = attachedFilesList?.getElementsByTagName("li");

    expect(attachemntsLength?.length).toBe(2);
  });

  it("Display correct number of recent tickets", async () => {
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

    const ticketTable = document.querySelectorAll("tbody")[0]; //TODO: Better selector for this
    const rows = ticketTable?.querySelectorAll(".rt-tr-group");

    expect(rows?.length).toBe(4);
  });

  it("Display correct number of recent invoices", async () => {
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

    const invoiceTable = document.querySelectorAll("tbody")[1];
    const rows = invoiceTable?.querySelectorAll(".rt-tr-group");

    expect(rows?.length).toBe(3);
  });
});
