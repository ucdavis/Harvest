import React, { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Ticket, CommonRouteParams } from "../types";
import { TicketTable } from "./TicketTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface Props {
  projectId?: string;
  compact: boolean;
}

export const RecentTicketsContainer = (props: Props) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const { team } = useParams<CommonRouteParams>();
  const maxRows = 5;

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/${team}/Ticket/GetList?projectId=${props.projectId}&maxRows=${maxRows}`
      );

      if (response.ok) {
        getIsMounted() && setTickets(await response.json());
      }
    };

    cb();
  }, [props.projectId, getIsMounted, team]);

  return (
    <div id="recentTicketContainer">
      <div className="row justify-content-between mt-4">
        <div className="col">
          <h3>Recent Tickets</h3>
        </div>
        <div className="col text-right">
          <Link
            className="mr-4"
            to={`/${team}/ticket/create/${props.projectId}`}
          >
            Create Ticket
          </Link>
          <Link to={`/${team}/ticket/List/${props.projectId}`}>View All</Link>
        </div>
      </div>

      <TicketTable compact={props.compact} tickets={tickets}></TicketTable>
    </div>
  );
};
