import React from "react";

import { Route, Switch, useLocation } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { ModalProvider } from "react-modal-hook";

import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";
import { ConditionalRoute } from "./ConditionalRoute";

import { AppNavBar } from "./Shared/AppNavBar";

import { ApprovalContainer } from "./Requests/ApprovalContainer";
import { ExpenseEntryContainer } from "./Expenses/ExpenseEntryContainer";
import { HomeContainer } from "./Home/HomeContainer";
import { UnbilledExpensesContainer } from "./Expenses/UnbilledExpensesContainer";
import { RequestContainer } from "./Requests/RequestContainer";
import { AccountChangeContainer } from "./Requests/AccountChangeContainer";
import { QuoteContainer } from "./Quotes/QuoteContainer";
import { QuoteDisplayContainer } from "./Quotes/QuoteDisplayContainer";
import { ProjectDetailContainer } from "./Projects/ProjectDetailContainer";
import { ProjectFields } from "./Projects/ProjectFields";
import { ProjectListContainer } from "./Projects/ProjectListContainer";
import { InvoiceDetailContainer } from "./Invoices/InvoiceDetailContainer";
import { InvoiceListContainer } from "./Invoices/InvoiceListContainer";
import { TicketListContainer } from "./Tickets/TicketListContainer";
import { TicketCreate } from "./Tickets/TicketCreate";
import { TicketsContainer } from "./Tickets/TicketsContainer";
import { TicketDetailContainer } from "./Tickets/TicketDetailContainer";
import { CloseoutContainer } from "./Closeout/CloseoutContainer";

// Global variable containing top-level app settings and info
declare var Harvest: AppContextShape;

function App() {
  return (
    <AppContext.Provider value={Harvest}>
      <ModalProvider>
        <Toaster />
        <Switch>
          <Route exact path="/" component={HomeContainer} />
          <Route
            exact
            path="/request/create/:projectId?"
            component={RequestContainer}
          />
          <ConditionalRoute
            roles={["PI"]}
            path="/request/approve/:projectId"
            component={ApprovalContainer}
          />
          <ConditionalRoute
            roles={["PI"]}
            path="/request/changeAccount/:projectId"
            component={AccountChangeContainer}
          />
          <Route
            path="/project/invoices/:projectId"
            component={InvoiceListContainer}
          />
          <ConditionalRoute
            roles={["FieldManager", "PI"]}
            path="/invoice/details/:projectId/:invoiceId"
            component={InvoiceDetailContainer}
          />
          <ConditionalRoute
            roles={["FieldManager", "Supervisor"]}
            path="/quote/create/:projectId"
            component={QuoteContainer}
          />
          <ConditionalRoute
            roles={["PI"]}
            path="/quote/details/:projectId"
            component={QuoteDisplayContainer}
          />
          <ConditionalRoute
            roles={["FieldManager"]}
            path="/project/closeout/:projectId"
            component={CloseoutContainer}
          />
          <ConditionalRoute
            exact
            roles={["FieldManager", "Supervisor"]}
            path="/project"
          >
            <ProjectListContainer projectSource="/api/Project/All" />
          </ConditionalRoute>
          <ConditionalRoute
            exact
            roles={["FieldManager", "Supervisor"]}
            path="/project/needsAttention"
          >
            <ProjectListContainer projectSource="/api/Project/RequiringManagerAttention" />
          </ConditionalRoute>
          <ConditionalRoute exact roles={["PI"]} path="/project/mine">
            <ProjectListContainer projectSource="/api/Project/GetMine" />
          </ConditionalRoute>
          <ConditionalRoute
            roles={["FieldManager", "Supervisor", "PI"]}
            path="/ticket/create/:projectId"
            component={TicketCreate}
          />
          <ConditionalRoute
            exact
            roles={["FieldManager", "Supervisor"]}
            path="/ticket/needsAttention"
          >
            <TicketListContainer projectSource="/api/ticket/RequiringManagerAttention" />
          </ConditionalRoute>
          <ConditionalRoute exact roles={["PI"]} path="/ticket/mine">
            <TicketListContainer projectSource="/api/ticket/RequiringPIAttention" />
          </ConditionalRoute>
          <Route path="/ticket/list/:projectId" component={TicketsContainer} />
          <Route
            path="/ticket/details/:projectId/:ticketId"
            component={TicketDetailContainer}
          />
          <Route
            path="/project/details/:projectId"
            component={ProjectDetailContainer}
          />
          <ConditionalRoute
            roles={["FieldManager", "Supervisor", "Worker"]}
            path="/expense/entry/:projectId?"
            component={ExpenseEntryContainer}
          />
          <Route
            path="/expense/unbilled/:projectId"
            component={UnbilledExpensesContainer}
          />
          <ConditionalRoute
            roles={["FieldManager", "Supervisor"]}
            exact
            path="/project/map"
            component={ProjectFields}
          />
          <Route path="*">
            <NoMatch />
          </Route>
        </Switch>
      </ModalProvider>
    </AppContext.Provider>
  );
}

const NoMatch = () => {
  let location = useLocation();

  return (
    <div>
      <h3>
        Page not found: <code>{location.pathname}</code>
      </h3>
    </div>
  );
};

export default App;
