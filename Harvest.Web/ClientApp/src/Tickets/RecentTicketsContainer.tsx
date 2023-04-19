import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";

interface Props {
  projectId?: string;
  team?: string;
  compact: boolean;
}

export const RecentTicketsContainer = (props: Props) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const maxRows = 5;

  const getIsMounted = useIsMounted();
  useEffect(() => {
    const cb = async () => {
      const response = await authenticatedFetch(
        `/api/Ticket/GetList?projectId=${props.projectId}&maxRows=${maxRows}`
      );

      if (response.ok) {
        getIsMounted() && setTickets(await response.json());
      }
    };

    cb();
  }, [props.projectId, getIsMounted]);

  return (
    <div id="recentTicketContainer">
      <div className="row justify-content-between mt-4">
        <div className="col">
          <h3>Recent Tickets</h3>
        </div>
        <div className="col text-right">
          <Link
            className="mr-4"
            to={`/${props.team}/ticket/create/${props.projectId}`}
          >
            Create Ticket
          </Link>
          <Link to={`/${props.team}/ticket/List/${props.projectId}`}>
            View All
          </Link>
        </div>
      </div>

      <TicketTable compact={props.compact} tickets={tickets}></TicketTable>
    </div>
  );
};
