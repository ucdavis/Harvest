import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Invoice } from "../types";
import { InvoiceTable } from "./InvoiceTable";
import { ShowFor } from "../Shared/ShowFor";

interface Props {
  projectId?: string;
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
    <ShowFor roles={["FieldManager", "PI"]} >
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
        </div>
        <div className="row justify-content-center">
          <InvoiceTable
            compact={props.compact}
            invoices={invoices.slice(0, 5)}
          ></InvoiceTable>
        </div>
      </div>
    </ShowFor>
  );
};
