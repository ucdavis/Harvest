import React from "react";

import { Route } from "react-router-dom";

import AppContext from "./Shared/AppContext";
import { AppContextShape } from "./types";
import { ConditionalRoute } from './ConditionalRoute'

import { ApprovalContainer } from "./Requests/ApprovalContainer";
import { ExpenseEntryContainer } from "./Expenses/ExpenseEntryContainer";
import { UnbilledExpensesContainer } from "./Expenses/UnbilledExpensesContainer";
import { RequestContainer } from "./Requests/RequestContainer";
import { AccountChangeContainer } from "./Requests/AccountChangeContainer";
import { QuoteContainer } from "./Quotes/QuoteContainer";
import { ProjectDetailContainer } from "./Projects/ProjectDetailContainer";
import { ProjectListContainer } from "./Projects/ProjectListContainer";
import { InvoiceDetailContainer } from "./Invoices/InvoiceDetailContainer";
import { InvoiceListContainer } from "./Invoices/InvoiceListContainer";
import { TicketCreate } from "./Tickets/TicketCreate";
import { TicketsContainer } from "./Tickets/TicketsContainer";
import { TicketDetailContainer } from "./Tickets/TicketDetailContainer";
import { Map } from "./Maps/Map";


// Global variable containing top-level app settings and info
declare var Harvest: AppContextShape;

function App() {
  return (
    <AppContext.Provider value={Harvest}>
      <Route exact path="/" component={Home} />
      <Route exact path="/home/spa" component={Spa} />
      <ConditionalRoute roles={["FieldManager"]} path="/request/create/:projectId?" component={RequestContainer} />
      <ConditionalRoute roles={["FieldManager"]} path="/request/approve/:projectId" component={ApprovalContainer} />
      <ConditionalRoute
        roles={["FieldManager"]}
        path="/request/changeAccount/:projectId"
        component={AccountChangeContainer}
      />
      <Route
        path="/project/invoices/:projectId"
        component={InvoiceListContainer}
      />
      <ConditionalRoute
        roles={["FieldManager"]}
        path="/invoice/details/:invoiceId"
        component={InvoiceDetailContainer}
      />
      <ConditionalRoute roles={["FieldManager"]} path="/quote/create/:projectId" component={QuoteContainer} />
      <Route exact path="/project" component={ProjectListContainer} />
      <ConditionalRoute roles={["FieldManager","Supervisor","PI"]} path="/ticket/create/:projectId" component={TicketCreate} />
      <Route path="/ticket/list/:projectId" component={TicketsContainer} />
      <Route path="/ticket/details/:projectId/:ticketId" component={TicketDetailContainer} />
      <Route
        path="/project/details/:projectId"
        component={ProjectDetailContainer}
      />
      <ConditionalRoute
        roles={["FieldManager", "Supervisor", "Worker"]}
        path="/expense/entry/:projectId?"
        component={ExpenseEntryContainer}
      />
       <Route path="/expense/unbilled/:projectId" component={UnbilledExpensesContainer} />
      <Route path="/home/map" component={Map} />
    </AppContext.Provider>
  );
}

const Home = () => <div>Home</div>;
const Spa = () => <div className="sassy">I am a SPA</div>;

export default App;
