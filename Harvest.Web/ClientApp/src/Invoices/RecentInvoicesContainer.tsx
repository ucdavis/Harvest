import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Invoice } from "../types";
import { InvoiceTable } from "./InvoiceTable";

interface Props {
  projectId: any;
  compact: boolean;
}

export const RecentInvoicesContainer = (props: Props) => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  useEffect(() => {
    const cb = async () => {
      // TODO: only fetch first 5 instead of chopping off client-side
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
        <div className="row">
          <div className="col-md-10">
            <h3>Recent Invoices</h3>
          </div>
          <div className="col-md-2">
            <Link to={`/project/invoices/${props.projectId}`}>View All</Link>
          </div>
        </div>
        <InvoiceTable
          compact={props.compact}
          invoices={invoices.slice(0, 5)}
        ></InvoiceTable>
      </div>
    </div>
  );
};
