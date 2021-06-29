import React, { useEffect, useState } from "react";
import { Link, useHistory } from "react-router-dom";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";

interface Props {
  projectId: any;
}


export const TicketListContainer = (props: Props) => {

  const [tickets, setTickets] = useState<Ticket[]>([]);

  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
        const response = await fetch(`/Ticket/List?projectId=${props.projectId}`);

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
              to={`/ticket/create/${props.projectId}`}
          >
                  Create Ticket
          </Link>
          <Link
              className="btn btn-primary btn-small mr-4"
              to={`/ticket/todo/${props.projectId}`}
          >
              View All Tickets
          </Link>
        <TicketTable tickets={tickets}></TicketTable>
      </div>
    </div>
  );
};
