import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { Invoice } from "../types";
import { InvoiceTable } from "./InvoiceTable";

interface RouteParams {
  projectId?: string;
}

export const InvoiceListContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/Project/Invoices/${projectId}`);

      if (response.ok) {
        setInvoices(await response.json());
      }
    };

    if (projectId) {
      cb();
    }
    cb();
  }, [projectId]);

  if (invoices.length === 0) {
    return <div>No invoices found</div>;
  }
  return (
    <div className="">
      <div className="card-content">
        <h3>Invoices</h3>
        <InvoiceTable invoices={invoices}></InvoiceTable>
      </div>
    </div>
  );
};
