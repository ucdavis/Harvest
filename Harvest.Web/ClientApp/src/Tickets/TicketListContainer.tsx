import React, { useEffect, useState } from "react";
import { Link, useHistory, useParams } from "react-router-dom";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";

interface Props {
  projectId: any;
}
interface RouteParams {
    projectId?: string;
}

export const TicketListContainer = (props: Props) => {
    const { projectId } = useParams<RouteParams>();
  const [tickets, setTickets] = useState<Ticket[]>([]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
        const response = await fetch(`/Ticket/ListTickets?projectId=${projectId}`);

      if (response.ok) {
        setTickets(await response.json());
      }
    };

    cb();
  }, [props.projectId]);

  return (
    <div className="">
      <div className="card-content">
              <h3>Last 5 Updated Tickets</h3>
          <Link
              className="btn btn-primary btn-small mr-4"
              to={`/ticket/create/${projectId}`}
          >
                  Create Ticket
          </Link>
          <Link
              className="btn btn-primary btn-small mr-4"
              to={`/ticket/todo/${projectId}`}
          >
              View All Tickets
          </Link>
        <TicketTable tickets={tickets}></TicketTable>
      </div>
    </div>
  );
};
