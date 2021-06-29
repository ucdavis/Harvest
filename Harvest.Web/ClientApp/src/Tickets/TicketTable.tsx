import { useMemo } from "react";
import { Cell, Column, TableState } from "react-table";
import { ReactTable } from "../Shared/ReactTable";
import { ReactTableUtil } from "../Shared/TableUtil";
import { Ticket } from "../types";

interface Props {
  tickets: Ticket[];
}

export const TicketTable = (props: Props) => {
  const ticketData = useMemo(() => props.tickets, [props.tickets]);
  const columns: Column<Ticket>[] = useMemo(
      () => [
          {
              Cell: (data: Cell<Ticket>) => (
                  <div>
                      <a href={`/ticket/details??projectId=${data.row.original.projectId}&id=${data.row.original.id}`}>
                          #{data.row.original.id}
                      </a>
                  </div>
              ),
              Header: " ",
              maxWidth: 150,
          },
          {
              Header: "Status",
              accessor: (row) => row.status,
          },
          {
              Header: "Updated",
              accessor: (row) => row.updatedOn ? (new Date(row.updatedOn).toLocaleDateString()) : '',
},
      {
        Header: "Subject",
        accessor: (row) => row.name,
      },
    ],
    []
  );

    const initialState: Partial<TableState<any>> = {
        sortBy: [{ id: "Updated", desc: true }],
      pageSize: ReactTableUtil.getPageSize(),
  };

  return (
    <ReactTable
      columns={columns}
      data={ticketData}
      initialState={initialState}
    />
  );
};
