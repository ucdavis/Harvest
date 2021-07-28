import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";

interface Props {
  projectId?: string;
  compact: boolean;
}

export const RecentTicketsContainer = (props: Props) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);
  const maxRows = 5;

  useEffect(() => {
    const cb = async () => {
      const response = await fetch(
        `/Ticket/GetList?projectId=${props.projectId}&maxRows=${maxRows}`
      );

      if (response.ok) {
        setTickets(await response.json());
      }
    };

    cb();
  }, [props.projectId]);

  return (
    <div className="card-content">
      <div className="row justify-content-between">
        <h3>Recent Tickets</h3>
        <div>
          <Link className="mr-4" to={`/ticket/create/${props.projectId}`}>
            Create Ticket
          </Link>
          <Link to={`/ticket/List/${props.projectId}`}>View All</Link>
        </div>
      </div>
      <div className="row justify-content-center">
        <TicketTable compact={props.compact} tickets={tickets}></TicketTable>
      </div>
    </div>
  );
};
