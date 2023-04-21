import React, { useEffect, useState } from "react";

import { Ticket } from "../types";
import { TicketTable } from "./TicketTable";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../Util/Api";
import { useParams } from "react-router";

interface RouteParams {
  team?: string;
}

interface Props {
  projectSource: string;
  hasTeamRoute: boolean;
}

export const TicketListContainer = (props: Props) => {
  const [tickets, setTickets] = useState<Ticket[]>([]);

  const { team } = useParams<RouteParams>();

  const getIsMounted = useIsMounted();
  useEffect(() => {
    // get rates so we can load up all expense types and info
    const cb = async () => {
      let url = props.projectSource;
      if (props.hasTeamRoute) {
        url = `/api/${team}${props.projectSource}`;
      }
      const response = await authenticatedFetch(url);

      if (response.ok) {
        getIsMounted() && setTickets(await response.json());
      }
    };

    cb();
  }, [getIsMounted, props.projectSource, props.hasTeamRoute, team]);

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
