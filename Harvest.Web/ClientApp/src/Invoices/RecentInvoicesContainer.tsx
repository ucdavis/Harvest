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
  const { team, shareId } = useParams<CommonRouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      //alert("Shareid" + shareId);
      // TODO: only fetch first 5 instead of chopping off client-side
      const response = await authenticatedFetch(
        `/api/${team}/Invoice/List/?projectId=${props.projectId}&maxRows=5&shareId=${shareId}`
      );

      if (response.ok) {
        getIsMounted() && setInvoices(await response.json());
      }
    };

    cb();
  }, [props.projectId, getIsMounted, team]);

  return (
    <ShowFor roles={["FieldManager", "Supervisor", "PI", "Finance", "Shared"]}>
      <div id="recentInvoiceContainer">
        <div className="row justify-content-between">
          <div className="col">
            <h3>Recent Invoices</h3>
          </div>
          <div className="col text-right">
            <Link
              to={
                shareId
                  ? `/${team}/project/invoices/${props.projectId}/${shareId}`
                  : `/${team}/project/invoices/${props.projectId}`
              }
            >
              View All
            </Link>
          </div>
        </div>

        <InvoiceTable
          compact={props.compact}
          invoices={invoices}
          shareId={shareId}
        ></InvoiceTable>
      </div>
    </ShowFor>
  );
};
