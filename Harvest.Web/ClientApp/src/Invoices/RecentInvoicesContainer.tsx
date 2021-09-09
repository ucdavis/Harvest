import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Invoice } from "../types";
import { InvoiceTable } from "./InvoiceTable";
import { ShowFor } from "../Shared/ShowFor";
import { useIsMounted } from "../Shared/UseIsMounted";

interface Props {
  projectId?: string;
  compact: boolean;
}

export const RecentInvoicesContainer = (props: Props) => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      // TODO: only fetch first 5 instead of chopping off client-side
        const response = await fetch(`/Invoice/List/?projectId=${props.projectId}`);

      if (response.ok) {
        getIsMounted() && setInvoices(await response.json());
      }
    };

    cb();
  }, [props.projectId, getIsMounted]);

  return (
    <ShowFor roles={["FieldManager", "Supervisor", "PI"]}>
      <div className="card-content">
        <div className="row justify-content-between">
          <h3>Recent Invoices</h3>
          <Link to={`/project/invoices/${props.projectId}`}>View All</Link>
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
