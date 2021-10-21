import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";
import { useIsMounted } from "../Shared/UseIsMounted";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

interface Props {
  projectSource: string;
}

export const TicketListContainer = (props: Props) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      const response = await fetch(props.projectSource);

      if (response.ok) {
        getIsMounted() && setTickets(await response.json());
      }
    };

    cb();
  }, [getIsMounted, props.projectSource]);

  return (
    <div>
      <div className="row justify-content-between mb-3">
        <div className="col">
          <h1>Open Tickets</h1>
        </div>
      </div>

      {tickets.length > 0 ? (
        <TicketTable tickets={tickets}></TicketTable>
      ) : (
        <div className="alert alert-info">No tickets found</div>
      )}
    </div>
  );
};