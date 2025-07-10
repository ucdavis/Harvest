import React from "react";

import { Route, Switch, useLocation } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { ModalProvider } from "react-modal-hook";

import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";
import { AppNav } from "./AppNav";
import { ConditionalRoute } from "./ConditionalRoute";

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
import { HistoryListContainer } from "./Histories/HistoryListContainer";
import { TicketListContainer } from "./Tickets/TicketListContainer";
import { TicketCreate } from "./Tickets/TicketCreate";
import { TicketsContainer } from "./Tickets/TicketsContainer";
import { TicketDetailContainer } from "./Tickets/TicketDetailContainer";
import { CloseoutContainer } from "./Closeout/CloseoutContainer";
import { CloseoutConfirmationContainer } from "./Closeout/CloseoutConfirmationContainer";
import { AdhocProject } from "./Projects/AdhocProject";
import { TeamPicker } from "./Teams/TeamPicker";

// Global variable containing top-level app settings and info
declare var Harvest: AppContextShape;

function App() {
  return (
    <AppContext.Provider value={Harvest}>
      <ModalProvider>
        <Toaster />
        <AppNav />
        <main role="main" className="main-content-wrapper container">
          <Switch>
            {/* Match any server-side routes and send empty content to let MVC return the view details */}
            <Route
              path="/:team/(rate|permissions|crop|help|report)"
              component={Empty}
            />
            <Route path="/(account|home|system|help)" component={Empty} />
            {/* Home route */}
            <Route exact path="/" component={HomeContainer} />
            {/* Visitors with non-PI roles will require selecting a team */}
            <Route exact path="/team" component={TeamPicker} />
            <Route exact path="/:team/team/" component={HomeContainer} />
            {/* Creating a new request requires first picking a team */}
            <Route exact path="/request/create/" component={TeamPicker} />
            <Route
              exact
              path="/:team/request/create/:projectId?"
              component={RequestContainer}
            />
            <ConditionalRoute
              roles={["PI", "Finance"]}
              path="/:team/request/approve/:projectId"
              component={ApprovalContainer}
            />
            <ConditionalRoute
              roles={["PI", "Finance"]}
              path="/:team/request/changeAccount/:projectId"
              component={AccountChangeContainer}
            />
            <Route
              path="/:team/project/invoices/:projectId/:shareId?"
              component={InvoiceListContainer}
            />
            <Route
              path="/:team/project/history/:projectId"
              component={HistoryListContainer}
            />
            <ConditionalRoute
              roles={["FieldManager", "PI", "Finance", "Shared"]}
              path="/:team/invoice/details/:projectId/:invoiceId/:shareId?"
              component={InvoiceDetailContainer}
            />
            <ConditionalRoute
              roles={["FieldManager", "Supervisor"]}
              path="/:team/quote/create/:projectId"
              component={QuoteContainer}
            />
            <ConditionalRoute
              roles={["PI", "FieldManager", "Finance", "Shared"]}
              path="/:team/quote/details/:projectId/:shareId?"
              component={QuoteDisplayContainer}
            />
            <ConditionalRoute
              roles={["FieldManager"]}
              path="/:team/project/closeout/:projectId"
              component={CloseoutContainer}
            />
            <ConditionalRoute
              roles={["FieldManager"]}
              path="/:team/project/adhocproject"
              component={AdhocProject}
            />
            <ConditionalRoute
              roles={["PI"]}
              path="/:team/project/closeoutconfirmation/:projectId"
              component={CloseoutConfirmationContainer}
            />
            <ConditionalRoute exact roles={["PI"]} path="/:team?/project/mine">
              <ProjectListContainer
                projectSource="/api/Project/GetMine"
                hasTeamRoute={false}
              />
            </ConditionalRoute>
            <ConditionalRoute
              roles={["FieldManager", "Supervisor", "PI"]}
              path="/:team/ticket/create/:projectId"
              component={TicketCreate}
            />
            {/* admin routes requiring team context */}
            <ConditionalRoute
              exact
              roles={["FieldManager", "Supervisor"]}
              path="/:team/project"
            >
              <ProjectListContainer
                projectSource="Project/All"
                hasTeamRoute={true}
              />
            </ConditionalRoute>
            <ConditionalRoute
              exact
              roles={["FieldManager", "Supervisor"]}
              path="/:team/project/needsAttention"
            >
              <ProjectListContainer
                projectSource="Project/RequiringManagerAttention"
                hasTeamRoute={true}
              />
            </ConditionalRoute>
            <ConditionalRoute
              exact
              roles={["FieldManager", "Supervisor"]}
              path="/:team/project/completed"
            >
              <ProjectListContainer
                projectSource="Project/GetCompleted"
                hasTeamRoute={true}
              />
            </ConditionalRoute>
            <ConditionalRoute
              exact
              roles={["FieldManager", "Supervisor"]}
              path="/:team/ticket/needsAttention"
            >
              <TicketListContainer
                projectSource="ticket/RequiringManagerAttention"
                hasTeamRoute={true}
              />
            </ConditionalRoute>
            <ConditionalRoute exact roles={["PI"]} path="/:team?/ticket/mine">
              <TicketListContainer
                projectSource="/api/ticket/RequiringPIAttention"
                hasTeamRoute={false}
              />
            </ConditionalRoute>
            <Route
              path="/:team/ticket/list/:projectId"
              component={TicketsContainer}
            />
            <Route
              path="/:team/ticket/details/:projectId/:ticketId"
              component={TicketDetailContainer}
            />
            <Route
              path="/:team/project/details/:projectId/:shareId?"
              component={ProjectDetailContainer}
            />
            <ConditionalRoute
              roles={["FieldManager", "Supervisor", "Worker"]}
              path="/:team/expense/entry/:projectId?"
              component={ExpenseEntryContainer}
            />
            <Route
              path="/:team/expense/unbilled/:projectId/:shareId?"
              component={UnbilledExpensesContainer}
            />
            <ConditionalRoute
              roles={["FieldManager", "Supervisor"]}
              exact
              path="/:team/project/map"
              component={ProjectFields}
            />
            <Route path="*">
              <NoMatch />
            </Route>
          </Switch>
        </main>
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

// Non-react routes can return nothing since the content will come from the server
const Empty = () => <></>;

export default App;
