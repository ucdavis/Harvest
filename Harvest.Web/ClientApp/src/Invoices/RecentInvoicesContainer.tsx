import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Invoice, CommonRouteParams } from "../types";
import { InvoiceTable } from "./InvoiceTable";
import { ShowFor } from "../Shared/ShowFor";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface Props {
  projectId?: string;
  compact: boolean;
}

export const RecentInvoicesContainer = (props: Props) => {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const { team } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      // TODO: only fetch first 5 instead of chopping off client-side
      const response = await authenticatedFetch(
        `/api/${team}/Invoice/List/?projectId=${props.projectId}&maxRows=5`
      );

      if (response.ok) {
        getIsMounted() && setInvoices(await response.json());
      }
    };

    cb();
  }, [props.projectId, getIsMounted, team]);

  return (
    <ShowFor roles={["FieldManager", "Supervisor", "PI", "Finance"]}>
      <div id="recentInvoiceContainer">
        <div className="row justify-content-between">
          <div className="col">
            <h3>Recent Invoices</h3>
          </div>
          <div className="col text-right">
            <Link to={`/${team}/project/invoices/${props.projectId}`}>
              View All
            </Link>
          </div>
        </div>

        <InvoiceTable
          compact={props.compact}
          invoices={invoices}
        ></InvoiceTable>
      </div>
    </ShowFor>
  );
};
