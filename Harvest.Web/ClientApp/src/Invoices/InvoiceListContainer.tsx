import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Invoice } from "../types";
import { InvoiceTable } from "../Invoices/InvoiceTable";

interface Props {
  projectId: any;
}

export const InvoiceListContainer = (props: Props) => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(`/Project/Invoices/${props.projectId}`);

      if (response.ok) {
        setInvoices(await response.json());
      }
    };

    cb();
  }, [props.projectId]);

  return (
    <div className="">
      <div className="card-content">
        <h3>Invoices</h3>
        <InvoiceTable invoices={invoices}></InvoiceTable>
      </div>
    </div>
  );
};
