import React from "react";
import { render, unmountComponentAtNode } from "react-dom";
import { MemoryRouter, Route } from "react-router-dom";
import { act } from "react-dom/test-utils";

import { ProjectDetailContainer } from "./ProjectDetailContainer";
import {
  fakeAppContext,
  fakeInvoices,
  fakeProject,
  fakeTickets,
} from "../Test/mockData";

let container: Element;

// jest.mock("../Shared/FileUpload", () => {
//   return {
//     default: () => {
//       return <div id="FileUpload">FileUpload</div>;
//     },
//   };
// });

// jest.mock("../Shared/FileUpload", () => {
//   return {
//     default: () => {
//       return <div id="FileUpload">FileUpload</div>;
//     },
//   };
// });

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
        <MemoryRouter initialEntries={["/project/details/3"]}>
          <Route path="/project/details/:projectId">
            <ProjectDetailContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const messageContent = container.querySelector("div")?.textContent;
    expect(messageContent).toContain("Loading");
  });

  it("Load details", async () => {
    await act(async () => {
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

      global.fetch = jest
        .fn()
        .mockImplementationOnce(() => Promise.resolve(projectResponse))
        .mockImplementationOnce(() => Promise.resolve(unbilledResponse))
        .mockImplementationOnce(() => Promise.resolve(fileResponse))
        .mockImplementationOnce(() => Promise.resolve(invoiceResponse))
        .mockImplementationOnce(() => Promise.resolve(ticketResponses));

      render(
        <MemoryRouter initialEntries={["/project/details/3"]}>
          <Route path="/project/details/:projectId">
            <ProjectDetailContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const fieldTitle = container.querySelector("#request-title")?.textContent;
    expect(fieldTitle).toContain("Field Request #3");
  });
});
