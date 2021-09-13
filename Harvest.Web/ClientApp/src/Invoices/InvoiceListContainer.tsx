import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Invoice } from "../types";
import { InvoiceTable } from "./InvoiceTable";
import { useIsMounted } from "../Shared/UseIsMounted";

interface RouteParams {
  projectId?: string;
}

export const InvoiceListContainer = () => {
  const { projectId } = useParams<RouteParams>();
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/Invoice/List/?projectId=${projectId}`);

      if (response.ok) {
        getIsMounted() && setInvoices(await response.json());
      }
    };

    if (projectId) {
      cb();
    }
  }, [projectId, getIsMounted]);

  if (invoices.length === 0) {
    return <div>No invoices found</div>;
  }
  return (
    <div className="">
      <div className="card-content">
        <h1>
          Invoices for{" "}
          <Link to={`/project/details/${projectId}`}>Project {projectId}</Link>
        </h1>
        <InvoiceTable invoices={invoices}></InvoiceTable>
      </div>
    </div>
  );
};
