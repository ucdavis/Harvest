import React from "react";

import { Route } from "react-router-dom";

import { ApprovalContainer } from "./Requests/ApprovalContainer";
import { ExpenseEntryContainer } from "./Expenses/ExpenseEntryContainer";
import { RequestContainer } from "./Requests/RequestContainer";
import { AccountChangeContainer } from "./Requests/AccountChangeContainer";
import { QuoteContainer } from "./Quotes/QuoteContainer";
import { ProjectDetailContainer } from "./Projects/ProjectDetailContainer";
import { ProjectListContainer } from "./Projects/ProjectListContainer";
import { InvoiceDetailContainer } from "./Invoices/InvoiceDetailContainer";
import { TicketContainer } from "./Tickets/TicketContainer";
import { Map } from "./Maps/Map";

function App() {
  return (
    <>
      <Route exact path="/" component={Home} />
      <Route exact path="/home/spa" component={Spa} />
      <Route path="/request/create/:projectId?" component={RequestContainer} />
      <Route path="/request/approve/:projectId" component={ApprovalContainer} />
      <Route path="/request/changeAccount/:projectId" component={AccountChangeContainer} />
      <Route path="/invoice/details/:invoiceId" component={InvoiceDetailContainer} />
      <Route path="/quote/create/:projectId" component={QuoteContainer} />
      <Route exact path="/project" component={ProjectListContainer} />
      <Route path="/ticket/create/:projectId" component={TicketContainer} />
      <Route
        path="/project/details/:projectId"
        component={ProjectDetailContainer}
      />
      <Route
        path="/expense/entry/:projectId?"
        component={ExpenseEntryContainer}
      />
      <Route path="/home/map" component={Map} />
    </>
  );
}

const Home = () => <div>Home</div>;
const Spa = () => <div className="sassy">I am a SPA</div>;

export default App;
