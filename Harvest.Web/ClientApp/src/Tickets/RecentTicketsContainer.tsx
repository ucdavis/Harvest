import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";

interface Props {
    projectId: string;
    compact: boolean;
}


export const RecentTicketsContainer = (props: Props) => {

    const [tickets, setTickets] = useState<Ticket[]>([]);
    const maxRows = 5;

  useEffect(() => {
      const cb = async () => {
        const response = await fetch(`/Ticket/GetList?projectId=${props.projectId}&maxRows=${maxRows}`);

      if (response.ok) {
        setTickets(await response.json());
      }
    };

    cb();
  }, [props.projectId]);

  return (
    <div className="">
      <div className="card-content">
              <h3>Last {maxRows} Updated Tickets</h3>
          <Link
              className="btn btn-primary btn-small mr-4"
              to={`/ticket/create/${props.projectId}`}
          >
                  Create Ticket
          </Link>
          <Link
              to={`/ticket/List/${props.projectId}`}
          >
              View All
          </Link>
        <TicketTable compact={props.compact} tickets={tickets}></TicketTable>
      </div>
    </div>
  );
};
